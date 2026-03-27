using Sandbox.Builder;

namespace BspImport.Builder;

public partial class MapBuilder
{
	public void BuildModelMeshes()
	{
		Log.Info( $"Building PolygonMeshes..." );

		var modelCount = Context.Models?.Length ?? 0;

		if ( modelCount <= 0 )
		{
			Log.Error( $"Unable to BuildPolygonMeshes, Context has no Models!" );
			return;
		}

		var polyMeshes = new PolygonMesh[modelCount];

		for ( int i = 0; i < modelCount; i++ )
		{
			var polyMesh = ConstructModel( i );

			if ( polyMesh is null )
				continue;

			polyMeshes[i] = polyMesh;
		}

		Context.CachedPolygonMeshes = polyMeshes;

		Log.Info( $"Done Building PolygonMeshes." );
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

	private IEnumerable<PolygonMesh?> ConstructDisplacementMeshes()
	{
		HashSet<ushort> DisplacementIndices = new();

		for ( short i = 0; i < Context.Geometry.DisplacementInfoCount; i++ )
		{
			Context.Geometry.TryGetDisplacementInfo( i, out var dispInfo );

			DisplacementIndices.Add( dispInfo.MapFace );
		}

		// create one mesh per displacement
		foreach ( ushort dispIndex in DisplacementIndices )
		{
			var dispMesh = ConstructDisplacement( dispIndex );
			if ( dispMesh is not null )
			{
				if ( dispMesh.FaceHandles.Any() )
				{
					yield return dispMesh;
				}
			}
		}
	}

	private IEnumerable<PolygonMesh?> ConstructWorldspawn()
	{
		var geo = Context.Geometry;

		if ( !Context.HasCompleteGeometry( out geo ) )
		{
			Log.Error( $"Failed constructing worldspawn geometry! No valid geometry in Context!" );
			yield return null;
		}

		// construct world mesh faces from bsp tree
		var faces = TreeParse.ParseTreeFaces( Context );

		if ( faces.Count == 0 )
		{
			Log.Error( $"Failed constructing worldspawn geometry! No faces in tree!" );
			yield return null;
		}

		// chunk tree faces into batches for MeshComponent
		foreach ( var chunk in faces.Chunk( Context.Settings.ChunkSize ) )
		{
			var polyMesh = new PolygonMesh();

			foreach ( var face in chunk )
			{
				if ( !geo.TryGetFace( face, out var f ) )
					continue;

				polyMesh.AddSplitMeshFace( Context, face );
			}

			if ( polyMesh is not null )
			{
				if ( polyMesh.FaceHandles.Any() )
					yield return polyMesh;
			}
		}
	}

	private PolygonMesh ConstructDisplacement( ushort faceIndex )
	{
		var mesh = new PolygonMesh();

		mesh.AddDisplacementMesh( Context, faceIndex );

		return mesh;
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

		var polyMesh = new PolygonMesh();

		var faces = GetFaceIndices( firstFaceIndex, faceCount );

		// invalid world mesh
		if ( faces.Count() <= 0 )
			return null;

		// build all split faces
		foreach ( var faceIndex in faces )
		{
			polyMesh.AddSplitMeshFace( Context, faceIndex );
		}

		//Log.Info( $"face count: {faces.Length}" );
		//Log.Info( $"poly mesh faces: {polyMesh.Faces.Count()}" );
		//Log.Info( $"poly mesh vertices: {polyMesh.Vertices.Count()}" );
		//Log.Info( $"------------" );

		//// no valid faces in mesh
		//if ( !polyMesh.Faces.Any() )
		//{
		//	Log.Error( $"ConstructPolygonMesh failed, [{firstFaceIndex}, {faceCount}] has no valid faces!" );
		//	return null;
		//}

		return polyMesh;
	}

	/// <summary>
	/// Gather all unique face indices from a firstFace index and a face count. Skips displacement faces.
	/// </summary>
	/// <param name="firstFaceIndex"></param>
	/// <param name="faceCount"></param>
	/// <returns></returns>
	/// <exception cref="Exception"></exception>
	private int[] GetFaceIndices( int firstFaceIndex, int faceCount )
	{
		var geo = Context.Geometry;
		if ( !geo.IsValid() )
		{
			throw new Exception( "No valid map geometry to construct!" );
		}

		var faces = new HashSet<int>();

		for ( int i = 0; i < faceCount; i++ )
		{
			var faceIndex = firstFaceIndex + i;

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
