using Microsoft.CodeAnalysis.CSharp.Syntax;
using SkiaSharp;
using System.Collections.Generic;
using System.Diagnostics;
using System.Diagnostics.Metrics;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace BspImport.Builder;

public partial class MapBuilder
{
	/// <summary>
	/// Builds cached PolygonMeshes for bsp models, skips index 0 (worldspawn).
	/// </summary>
	/// <param name="progress">Current progress section</param>
	/// <param name="token">Progress cancellation token</param>
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

		// i = 1 to skip 0 (worldspawn)
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

	/// <summary>
	/// Find all areas with a sky_camera entity in them.
	/// </summary>
	private List<short> FindSkyboxAreas()
	{
		var result = new List<short>();

		ArgumentNullException.ThrowIfNull( Context.Entities );

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
	/// Constructs quad corners for a plane.
	/// </summary>
	/// <param name="p"></param>
	private List<Vector3> BuildBasePolygon( Plane p )
	{
		var normal = p.Normal;

		// pick a tangent
		Vector3 up = Math.Abs( normal.z ) > 0.99f ? Vector3.Forward : Vector3.Up;
		Vector3 right = Vector3.Cross( up, normal ).Normal;
		up = Vector3.Cross( normal, right );

		float size = 16384f; // big enough for BSP scale

		Vector3 center = normal * p.Distance;
		return new List<Vector3>
		{
			center + (-right - up) * size,
			center + ( right - up) * size,
			center + ( right + up) * size,
			center + (-right + up) * size,
		};
	}

	List<Vector3> ClipPolygon( List<Vector3> input, Plane plane )
	{
		var output = new List<Vector3>();

		for ( int i = 0; i < input.Count; i++ )
		{
			var a = input[i];
			var b = input[(i + 1) % input.Count];

			float da = Vector3.Dot( plane.Normal, a ) - plane.Distance;
			float db = Vector3.Dot( plane.Normal, b ) - plane.Distance;

			const float EPS = 0.001f;

			bool ina = da <= EPS;
			bool inb = db <= EPS;

			if ( ina && inb )
			{
				output.Add( b );
			}
			else if ( ina && !inb )
			{
				float t = da / (da - db);
				output.Add( a + (b - a) * t );
			}
			else if ( !ina && inb )
			{
				float t = da / (da - db);
				output.Add( a + (b - a) * t );
				output.Add( b );
			}
		}

		return output;
	}

	private void BuildClipBrushes( GameObject _parent )
	{
		if ( Context.Brushes is not null && Context.BrushSides is not null && Context.Planes is not null )
		{
			var clipBrushes = Context.Brushes.Where( b => b.IsClipBrush ).ToList();
			if ( clipBrushes.Count == 0 )
				return;

			int count = 0;
			var parent = new GameObject( _parent, true, "Clip Brushes" );
			foreach ( var brush in clipBrushes )
			{
				if ( !brush.IsClipBrush )
					continue;

				var clipObject = new GameObject( parent, true, $"Clip Brush {count}" );
				var clipMesh = new PolygonMesh();

				// get brush sides
				var firstSide = brush.FirstSide;
				var numSides = brush.NumSides;

				for ( int i = 0; i < numSides; i++ )
				{
					int sideIndex = firstSide + i;

					var brushSide = Context.BrushSides[sideIndex];
					var planeIndex = brushSide.PlaneNum;

					var plane = Context.Planes[planeIndex];

					// build quad
					var poly = BuildBasePolygon( plane );
					for ( int j = 0; j < numSides; j++ )
					{
						int otherSideIndex = firstSide + j;

						if ( sideIndex == otherSideIndex )
							continue;

						var other = Context.Planes[Context.BrushSides[otherSideIndex].PlaneNum];

						poly = ClipPolygon( poly, other );

						if ( poly.Count == 0 )
						{
							break;
						}
					}

					if ( poly.Count >= 3 )
					{
						var hVerts = clipMesh.AddVertices( poly.ToArray() );
						var hFace = clipMesh.AddFace( hVerts );
						clipMesh.SetFaceMaterial( hFace, "materials/tools/toolsclip.vmat" );
						clipMesh.TextureAlignToGrid( Transform.Zero, hFace );
					}
				}

				var meshComp = clipObject.Components.Create<MeshComponent>();
				meshComp.Mesh = clipMesh;
				meshComp.HideInGame = true;
				meshComp.RenderType = ModelRenderer.ShadowRenderType.Off;

				CenterMeshOrigin( meshComp );

				count++;
			}
		}
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

			foreach ( var meshResult in worldspawnMeshes )
			{
				if ( token.IsCancellationRequested )
				{
					return;
				}
				var mesh = meshResult.Mesh;
				if ( mesh is null )
					continue;


				var meshName = $"Mesh {count}";

				if ( meshResult.IsWater )
				{
					meshName = $"Water Mesh";
				}

				var meshComp = ConstructMesh( meshParent, meshName, mesh );
				meshComp.Collision = meshResult.IsWater ? MeshComponent.CollisionType.None : MeshComponent.CollisionType.Mesh;

				progress.Current = count + displacementMeshes.Count;
				count++;

				if ( count % meshesPerFrame == 0 )
				{
					await GameTask.Yield();
				}
			}
		}

		BuildClipBrushes( parent );
	}

	private MeshComponent ConstructMesh( GameObject parent, string name, PolygonMesh mesh )
	{
		using var scope = parent.Scene.Push();
		var meshObj = new GameObject( parent, true, name );
		var meshComp = meshObj.Components.Create<MeshComponent>();
		meshComp.Mesh = mesh;
		CenterMeshOrigin( meshComp );

		return meshComp;
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

			if ( Context.BuildSettings.CullSkybox && Context.SkyboxAreas.Contains( dispLeaf.Area ) )
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

	private bool IsWaterSurface( ushort faceIndex )
	{
		if ( !Context.HasCompleteGeometry( out var geo ) )
			return false;

		if ( !geo.TryGetFace( faceIndex, out var face ) )
			return false;

		var surfaceFlags = face.GetSurfaceFlags( Context );
		return (surfaceFlags & SurfaceFlags.Warp) != 0;
	}

	public class WorldspawnMesh
	{
		public PolygonMesh? Mesh { get; set; }
		public bool IsTranslucent { get; set; }
		public bool IsWater { get; set; }
	}

	/// <summary>
	/// Construct PolygonMeshes from the bsp-tree, chunked into individual Meshes based on Settings.ChunkSize and surface properties such as Translucent or Water.
	/// </summary>
	/// <returns></returns>
	private async Task<List<WorldspawnMesh>> ConstructWorldspawnMeshes( CancellationToken token, IProgressSection progress )
	{
		var geo = Context.Geometry;

		var meshes = new List<WorldspawnMesh>();

		if ( !Context.HasCompleteGeometry( out geo ) )
		{
			Log.Error( $"Failed constructing worldspawn geometry! No valid geometry in Context!" );
			return meshes;
		}

		// construct world mesh faces from bsp tree
		var result = TreeParse.GetUniqueWorldspawnFaces();

		var faceIndices = result.FaceIndices;
		var waterFaceIndices = faceIndices.Where( fi => IsWaterSurface( fi ) ).ToList();
		var solidFaces = faceIndices.Where( fi => !IsWaterSurface( fi ) ).ToList();

		if ( solidFaces.Count == 0 )
		{
			Log.Error( $"Failed constructing worldspawn geometry! No faces in tree!" );
			return meshes;
		}

		// spawn solid geometry first
		var chunks = solidFaces.Chunk( Context.BuildSettings.ChunkSize );

		if ( token.IsCancellationRequested )
			return meshes;

		Log.Info( "Constructing World Meshes..." );
		progress.Title = "Constructing World Meshes...";
		progress.TotalCount = chunks.Count();
		progress.Current = 0;

		// chunk tree faces into batches for MeshComponent
		foreach ( var chunk in chunks )
		{
			if ( token.IsCancellationRequested )
				return meshes;

			var mesh = new PolygonMesh();

			foreach ( ushort faceIndex in chunk )
			{
				if ( token.IsCancellationRequested )
					return meshes;

				if ( !geo.TryGetFace( faceIndex, out var face ) )
					continue;

				mesh.AddMeshFace( Context, face );
			}

			progress.Current++;

			if ( mesh.FaceHandles.Any() )
			{
				var meshResult = new WorldspawnMesh()
				{
					Mesh = mesh,
					IsTranslucent = false,
					IsWater = false
				};
				meshes.Add( meshResult );
			}

			await GameTask.Yield();
		}

		// add water surfaces as a mesh
		var waterMesh = new PolygonMesh();
		foreach ( var faceIndex in waterFaceIndices )
		{
			if ( !geo.TryGetFace( faceIndex, out var face ) )
				continue;

			waterMesh.AddMeshFace( Context, face );
		}
		if ( waterMesh.FaceHandles.Any() )
		{
			var meshResult = new WorldspawnMesh()
			{
				Mesh = waterMesh,
				IsTranslucent = true,
				IsWater = true
			};
			meshes.Add( meshResult );
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

		if ( !Context.HasCompleteGeometry( out var geo ) )
			return null;

		// models support int firstFace and faceCount for some reason, but faces are limited to ushort
		var faces = GetFaceIndices( (ushort)firstFaceIndex, (ushort)faceCount );

		// invalid world mesh
		if ( faces.Length <= 0 )
			return null;

		// build all split faces
		var polyMesh = new PolygonMesh();
		foreach ( ushort faceIndex in faces )
		{
			if ( !geo.TryGetFace( faceIndex, out var face ) )
				continue;

			polyMesh.AddMeshFace( Context, face );
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
