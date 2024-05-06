using Sandbox.Builder;

namespace BspImport.Builder;

public partial class MapBuilder
{
	public void BuildPolygonMeshes()
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

	private IEnumerable<PolygonMesh?> ConstructWorldspawn()
	{
		var geo = Context.Geometry;

		if ( geo.Vertices is null || geo.SurfaceEdges is null || geo.EdgeIndices is null || geo.Faces is null || geo.OriginalFaces is null )
		{
			Log.Error( $"Failed constructing worldspawn geometry! No valid geometry in Context!" );
			yield return null;
		}

		var faces = TreeParse.ParseTreeFaces( Context );

		if ( faces.Count == 0 )
		{
			Log.Error( $"Failed constructing worldspawn geometry! No faces in tree!" );
			yield return null;
		}


		// chunk tree faces into batches for MeshComponent
		foreach ( var chunk in faces.Chunk( 100 ) )
		{
			var polyMesh = new PolygonMesh();

			foreach ( var face in chunk )
			{
				polyMesh.AddSplitMeshFace( Context, face );
			}

			yield return polyMesh;
		}

		// clump all tree meshlets into worlspawn mesh
		//polyMesh.MergeVerticies = true;
		//foreach ( var face in faces )
		//{
		//	polyMesh.AddSplitMeshFace( Context, face );
		//}

		//yield return polyMesh;
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

		if ( Context.Models is null || geo.Vertices is null || geo.SurfaceEdges is null || geo.EdgeIndices is null || geo.Faces is null || geo.OriginalFaces is null )
		{
			throw new Exception( "No valid map geometry to construct!" );
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

		if ( Context.Models is null || geo.Vertices is null || geo.SurfaceEdges is null || geo.EdgeIndices is null || geo.Faces is null || geo.OriginalFaces is null )
		{
			throw new Exception( "No valid map geometry to construct!" );
		}

		var polyMesh = new PolygonMesh();

		var faces = GetFaceIndices( firstFaceIndex, faceCount );
		//Log.Info( $"gathered {faces.Length} faces" );

		// only displacements, probably
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

		if ( Context.Models is null || geo.Vertices is null || geo.SurfaceEdges is null || geo.EdgeIndices is null || geo.Faces is null || geo.OriginalFaces is null )
		{
			throw new Exception( "No valid map geometry to construct!" );
		}

		var faces = new HashSet<int>();

		for ( int i = 0; i < faceCount; i++ )
		{
			var faceIndex = firstFaceIndex + i;

			var face = geo.Faces[faceIndex];

			// skip faces with invalid area
			if ( face.Area <= 0 || face.Area.AlmostEqual( 0 ) )
			{
				//Log.Info( $"skipping face with invalid area: {faceIndex}" );
				continue;
			}

			// skip displacement faces, displacement info != 0 means it's a displacement
			var displacementInfoIndex = face.DisplacementInfo;
			if ( displacementInfoIndex >= 0 )
			{
				//Log.Info( $"skipping displacement face: {faceIndex}" );
				continue;
			}

			faces.Add( faceIndex );
		}

		return faces.ToArray();
	}
}
