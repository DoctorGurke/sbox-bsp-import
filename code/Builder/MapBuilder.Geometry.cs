namespace BspImport.Builder;

public partial class MapBuilder
{
	public void CachePolygonMeshes()
	{
		// caching is always done in parallel
		//ThreadSafe.AssertIsNotMainThread();

		Log.Info( $"Caching PolygonMeshes..." );

		var modelCount = Context.Models?.Length ?? 0;

		if ( modelCount <= 0 )
		{
			Log.Error( $"Unable to CachePolygonMeshes, Context has no Models!" );
			return;
		}

		var polyMeshes = new PolygonMesh[modelCount];

		for ( int i = 0; i < modelCount; i++ )
		{
			var origin = Vector3.Zero;
			var angles = Angles.Zero;

			// index 0 = worldspawn
			if ( i != 0 )
			{
				// get any entity with this model, needed to build uvs for brush entity meshes properly
				var entity = Context.Entities?.Where( x => x.Data.Where( x => x.Key == "model" ).FirstOrDefault().Value == $"*{i}" ).FirstOrDefault();

				// no entity found, don't bother
				if ( entity is null )
				{
					continue;
				}

				origin = entity.Position;
				angles = entity.Angles;
			}

			var polyMesh = ConstructModel( i, origin, angles );

			if ( polyMesh is null )
				continue;

			polyMeshes[i] = polyMesh;
		}

		Context.CachedPolygonMeshes = polyMeshes;

		Log.Info( $"Done Caching PolygonMeshes." );
	}

	private PolygonMesh? ConstructModel( int modelIndex, Vector3 origin, Angles angles )
	{
		// if model is already cached, throw
		if ( Context.CachedPolygonMeshes?[modelIndex] is not null )
		{
			throw new Exception( $"Trying to reconstruct already cached model with index: {modelIndex}!" );
		}

		var geo = Context.Geometry;

		if ( Context.Models is null || geo.VertexPositions is null || geo.SurfaceEdges is null || geo.EdgeIndices is null || geo.Faces is null || geo.OriginalFaces is null )
		{
			throw new Exception( "No valid map geometry to construct!" );
		}

		if ( modelIndex < 0 || modelIndex >= Context.Models.Length )
		{
			throw new Exception( $"Tried to construct map model with index: {modelIndex}. Exceeds available Models!" );
		}

		var model = Context.Models[modelIndex];

		var polyMesh = new PolygonMesh();

		// collect all original face indices and their split faces
		var faceCollection = new Dictionary<int, List<int>>();

		var faces = new HashSet<int>();

		for ( int i = 0; i < model.FaceCount; i++ )
		{
			var faceIndex = model.FirstFace + i;

			var face = geo.Faces[faceIndex];

			// skip faces with invalid area
			if ( face.Area <= 0 || face.Area.AlmostEqual( 0 ) )
				continue;

			faces.Add( faceIndex );

			//var oFaceIndex = face.OriginalFaceIndex;

			//// no texture info, skip face (SKIP, CLIP, INVISIBLE, etc)
			//if ( oFaceIndex < 0 || oFaceIndex >= Context.Geometry.OriginalFaces?.Length || face.TexInfo < 0 || face.TexInfo >= Context.TexInfo?.Length )
			//	continue;

			//// sync texinfo, original faces have screwed up texinfo, just take it from the split face
			//geo.OriginalFaces[oFaceIndex].TexInfo = face.TexInfo;

			//// associate split face to original face index
			//if ( faceCollection.TryGetValue( oFaceIndex, out var faces ) )
			//{
			//	faces.Add( faceIndex );
			//}
			//else
			//{
			//	// new split face collection
			//	var fCol = new List<int>();
			//	fCol.Add( faceIndex );

			//	faceCollection.Add( oFaceIndex, fCol );
			//}
		}

		// build all split faces
		foreach ( var faceIndex in faces )
		{
			polyMesh.AddSplitMeshFace( Context, faceIndex, origin, angles );
		}

		//// construct and add faces to poly mesh
		//foreach ( var faceEntry in faceCollection )
		//{
		//	// key is oFaceIndex
		//	var oFaceIndex = faceEntry.Key;

		//	foreach ( var faceIndex in faceEntry.Value )
		//	{
		//		polyMesh.AddSplitMeshFace( Context, faceIndex, origin, angles );
		//	}

		//	//see: PolygonMeshX
		//	//polyMesh.AddOriginalMeshFace( Context, oFaceIndex, origin, angles );
		//}

		// no valid faces in mesh
		if ( !polyMesh.Faces.Any() )
		{
			Log.Error( $"ConstructModel failed, no valid faces constructed!" );
			return null;
		}

		Log.Info( $"PolyMesh constructed for {modelIndex}." );

		return polyMesh;
	}
}
