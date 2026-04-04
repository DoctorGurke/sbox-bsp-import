using System.Threading;
using System.Threading.Tasks;
using System.Linq;
using System.Collections.Generic;
using System.Diagnostics.Metrics;
using System.Diagnostics;

namespace BspImport.Builder;

public partial class MapBuilder
{
	public async Task BuildModelMeshes( IProgressSection progress, CancellationToken token )
	{
		var modelCount = Context.Models?.Length ?? 0;

		if ( modelCount <= 0 )
		{
			Log.Error( $"Unable to build bsp models, Context has no Models!" );
			return;
		}

		Log.Info( $"Constructing {modelCount} Entity Models..." );
		progress.Title = $"Constructing {modelCount} Entity Models...";
		progress.TotalCount = modelCount;
		progress.Current = 0;

		var polyMeshes = new PolygonMesh[modelCount];

		for ( int i = 1; i < modelCount; i++ )
		{
			if ( token.IsCancellationRequested )
				return;

			var polyMesh = ConstructModel( i );
			progress.Current = i;

			if ( polyMesh is null )
				continue;

			polyMeshes[i] = polyMesh;

			await GameTask.Yield();
		}

		Context.CachedPolygonMeshes = polyMeshes;
	}

	private List<short> FindSkyboxAreas()
	{
		var result = new List<short>();
		if ( Context.Entities is null )
			return result;

		foreach ( var ent in Context.Entities )
		{
			if ( ent.ClassName != "sky_camera" )
				continue;

			var origin = ent.Position;

			var leafIndex = TreeParse.FindLeafIndex( origin );
			if ( leafIndex == -1 )
				continue;

			var leaf = Context.Leafs![leafIndex];
			result.Add( leaf.Area );
		}

		return result;
	}

	/// <summary>
	/// Builds the map world geometry of the current context. Brush entities require pre-built PolygonMeshes. See <see cref="BuildModelMeshes"/>.
	/// </summary>
	protected virtual async Task BuildWorldGeometry( GameObject parent, IProgressSection progress, int meshesPerFrame, CancellationToken token )
	{
		var displacementMeshes = await ConstructDisplacementMeshesAsync( token, progress, meshesPerFrame );

		if ( token.IsCancellationRequested )
			return;

		var worldspawnMeshes = await ConstructWorldspawnMeshes( token, progress );

		if ( token.IsCancellationRequested )
			return;

		Log.Info( "Building World..." );
		progress.Title = "Building World...";
		progress.TotalCount = displacementMeshes.Count + worldspawnMeshes.Count;

		if ( displacementMeshes.Count >= 0 )
		{
			var displacementParent = new GameObject( parent, true, "Displacements" );
			int count = 0;

			progress.Subtitle = $"Building {displacementMeshes.Count} Displacement Meshes";

			foreach ( var displacement in displacementMeshes )
			{
				try
				{
					if ( token.IsCancellationRequested )
					{
						return;
					}

					progress.Current = count;

					ConstructMesh( displacementParent, $"Displacement {count}", displacement );

					count++;

					if ( count % meshesPerFrame == 0 )
					{
						await GameTask.Yield();
					}
				}
				catch ( Exception )
				{
					Log.Error( "Failed building displacement!" );
					continue;
				}
			}
		}

		if ( worldspawnMeshes.Count >= 0 )
		{
			var meshParent = new GameObject( parent, true, "Meshes" );
			int count = 0;

			progress.Subtitle = $"Building {worldspawnMeshes.Count} World Meshes";

			foreach ( var mesh in worldspawnMeshes )
			{
				if ( token.IsCancellationRequested )
				{
					return;
				}

				progress.Current = count + displacementMeshes.Count;
				ConstructMesh( meshParent, $"Mesh {count}", mesh );
				count++;

				if ( count % meshesPerFrame == 0 )
				{
					await GameTask.Yield();
				}
			}
		}
	}

	private void ConstructMesh( GameObject parent, string name, PolygonMesh mesh )
	{
		using var scope = parent.Scene.Push();
		var meshObj = new GameObject( parent, true, name );
		var meshComp = meshObj.Components.Create<MeshComponent>();
		meshComp.Mesh = mesh;
		CenterMeshOrigin( meshComp );
	}

	static void CenterMeshOrigin( MeshComponent meshComponent )
	{
		if ( !meshComponent.IsValid() ) return;

		var mesh = meshComponent.Mesh;
		if ( mesh is null ) return;

		var children = meshComponent.GameObject.Children
			.Select( x => (GameObject: x, Transform: x.WorldTransform) )
			.ToArray();

		var world = meshComponent.WorldTransform;
		var bounds = mesh.CalculateBounds( world );
		var center = bounds.Center;
		var localCenter = world.PointToLocal( center );
		meshComponent.WorldPosition = center;
		meshComponent.Mesh.ApplyTransform( new Transform( -localCenter ) );
		meshComponent.RebuildMesh();

		foreach ( var child in children )
		{
			child.GameObject.WorldTransform = child.Transform;
		}
	}

	private async Task<List<PolygonMesh>> ConstructDisplacementMeshesAsync( CancellationToken token, IProgressSection progress, int meshesPerFrame = 16 )
	{
		// gather unique displacement face indices
		HashSet<ushort> dispIndices = new();

		for ( short i = 0; i < Context.Geometry.DisplacementInfoCount; i++ )
		{
			Context.Geometry.TryGetDisplacementInfo( i, out var dispInfo );

			dispIndices.Add( dispInfo.MapFace );
		}

		var displacements = new List<PolygonMesh>();
		if ( dispIndices.Count == 0 )
			return displacements;

		Log.Info( "Constructing Displacement Meshes..." );
		progress.Title = "Constructing Displacement Meshes...";
		progress.TotalCount = dispIndices.Count;

		int count = 0;
		foreach ( ushort dispFaceIndex in dispIndices )
		{
			if ( token.IsCancellationRequested )
				return displacements;

			var dispOrigin = DisplacementHelper.GetDisplacementOrigin( Context, dispFaceIndex );
			var dispLeafIndex = TreeParse.FindLeafIndex( dispOrigin!.Value );
			var dispLeaf = Context.Leafs![dispLeafIndex];

			if ( Context.Settings.CullSkybox && Context.SkyboxAreas.Contains( dispLeaf.Area ) )
				continue;

			// create one mesh per displacement
			var dispMesh = DisplacementHelper.CreateDisplacementMesh( Context, dispFaceIndex );
			if ( dispMesh is null )
				continue;

			if ( dispMesh.FaceHandles.Any() )
			{
				displacements.Add( dispMesh );
			}

			progress.Current = count;

			count++;
			if ( count % meshesPerFrame == 0 )
			{
				await GameTask.Yield();
			}
		}

		return displacements;
	}

	public static Vector2 GetTexCoords( ImportContext context, int texInfoIndex, Vector3 position, int width = 1024, int height = 1024 )
	{
		// validate texinfo availability and index
		if ( context.TexInfo is null || texInfoIndex < 0 || texInfoIndex >= context.TexInfo.Length )
			return default;

		var ti = context.TexInfo[texInfoIndex];

		if ( context.TexData is not null && ti.TexData >= 0 && ti.TexData < context.TexData.Length )
		{
			var texData = context.TexData[ti.TexData];
			width = texData.Width;
			height = texData.Height;
		}

		return ti.GetUvs( position, width, height );
	}

	/// <summary>
	/// Construct PolygonMeshes from the bsp-tree, chunked into individual Meshes based on Settings.ChunkSize.
	/// </summary>
	/// <returns></returns>
	private async Task<List<PolygonMesh>> ConstructWorldspawnMeshes( CancellationToken token, IProgressSection progress )
	{
		var geo = Context.Geometry;

		var meshes = new List<PolygonMesh>();

		if ( !Context.HasCompleteGeometry( out geo ) )
		{
			Log.Error( $"Failed constructing worldspawn geometry! No valid geometry in Context!" );
			return meshes;
		}

		// construct world mesh faces from bsp tree
		var result = TreeParse.GetUniqueWorldspawnFaces();

		var faceIndices = result.FaceIndices;

		if ( faceIndices.Count == 0 )
		{
			Log.Error( $"Failed constructing worldspawn geometry! No faces in tree!" );
			return meshes;
		}

		var chunks = faceIndices.Chunk( Context.Settings.ChunkSize );

		if ( token.IsCancellationRequested )
			return meshes;

		Log.Info( "Constructing World Meshes..." );
		progress.Title = "Constructing World Meshes...";
		progress.TotalCount = chunks.Count();
		progress.Current = 0;

		// chunk tree faces into batches for MeshComponent
		foreach ( var chunk in chunks )
		{
			var mesh = new PolygonMesh();

			foreach ( var face in chunk )
			{
				if ( !geo.TryGetFace( face, out var _ ) )
					continue;

				mesh.AddMeshFace( Context, face );
			}

			progress.Current++;

			if ( mesh.FaceHandles.Any() )
				meshes.Add( mesh );

			await GameTask.Yield();
		}

		return meshes;
	}

	/// <summary>
	/// Construct a PolygonMesh from a bsp model index.
	/// </summary>
	/// <param name="modelIndex"></param>
	/// <returns></returns>
	/// <exception cref="Exception"></exception>
	private PolygonMesh? ConstructModel( int modelIndex )
	{
		// return already cached mesh
		if ( Context.CachedPolygonMeshes?[modelIndex] is not null )
		{
			return Context.CachedPolygonMeshes[modelIndex];
		}

		var geo = Context.Geometry;

		if ( !Context.HasCompleteGeometry( out geo ) )
		{
			throw new Exception( "No valid map geometry to construct!" );
		}

		if ( Context.Models is null )
		{
			throw new Exception( "No valid models to construct!" );
		}

		if ( modelIndex < 0 || modelIndex >= Context.Models.Length )
		{
			throw new Exception( $"Tried to construct map model with index: {modelIndex}. Exceeds available Models!" );
		}

		var model = Context.Models[modelIndex];

		return ConstructPolygonMesh( model.FirstFace, model.FaceCount );
	}

	/// <summary>
	/// Construct a PolygonMesh from a firstFace index and face count.
	/// </summary>
	/// <param name="firstFaceIndex"></param>
	/// <param name="faceCount"></param>
	/// <returns></returns>
	/// <exception cref="Exception"></exception>
	private PolygonMesh? ConstructPolygonMesh( int firstFaceIndex, int faceCount )
	{
		if ( faceCount <= 0 )
			return null;

		//Log.Info( $"construct poly mesh: [{firstFaceIndex}, {faceCount}]" );

		var geo = Context.Geometry;
		if ( !geo.IsValid() )
		{
			throw new Exception( "No valid map geometry to construct!" );
		}

		// models support int firstFace and faceCount for some reason, but faces are limited to ushort
		var faces = GetFaceIndices( (ushort)firstFaceIndex, (ushort)faceCount );

		// invalid world mesh
		if ( faces.Length <= 0 )
			return null;

		// build all split faces
		var polyMesh = new PolygonMesh();
		foreach ( ushort faceIndex in faces )
		{
			polyMesh.AddMeshFace( Context, faceIndex );
		}

		return polyMesh;
	}

	/// <summary>
	/// Gather all unique face indices from a firstFace index and a face count. Skips displacement faces.
	/// </summary>
	/// <param name="firstFaceIndex"></param>
	/// <param name="faceCount"></param>
	/// <returns></returns>
	/// <exception cref="Exception"></exception>
	private ushort[] GetFaceIndices( ushort firstFaceIndex, ushort faceCount )
	{
		var geo = Context.Geometry;
		if ( !geo.IsValid() )
		{
			throw new Exception( "No valid map geometry to construct!" );
		}

		var faces = new HashSet<ushort>();

		for ( ushort i = 0; i < faceCount; i++ )
		{
			var faceIndex = firstFaceIndex;
			faceIndex += i;

			geo.TryGetFace( faceIndex, out var face );

			// skip faces with invalid area
			if ( face.Area <= 0 || face.Area.AlmostEqual( 0 ) )
			{
				//Log.Info( $"skipping face with invalid area: {faceIndex}" );
				continue;
			}

			// skip displacement faces, is this needed anymore?
			if ( face.DisplacementInfo >= 0 )
			{
				continue;
			}

			faces.Add( faceIndex );
		}

		return faces.ToArray();
	}
}
