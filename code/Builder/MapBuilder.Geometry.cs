using System.Security.Principal;

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

		Log.Info( $"Done Building PolygonMeshes." );
	}

	private PolygonMesh? ConstructWorldspawn()
	{
		var geo = Context.Geometry;

		if ( geo.Vertices is null || geo.SurfaceEdges is null || geo.EdgeIndices is null || geo.Faces is null || geo.OriginalFaces is null )
		{
			throw new Exception( "No valid map geometry to construct!" );
		}

		var faces = TreeParse.ParseTreeFaces( Context );

		if ( faces.Count == 0 )
		{
			Log.Error( $"Failed constructing worldspawn geometry! No faces in tree!" );
			return null;
		}

		var polyMesh = new PolygonMesh();

		// clump all tree meshlets into worlspawn mesh
		polyMesh.MergeVerticies = true;
		foreach ( var face in faces )
		{
			polyMesh.AddSplitMeshFace( Context, face, Vector3.Zero );
		}

		return polyMesh;
	}

	private PolygonMesh? ConstructModel( int modelIndex, Vector3 origin, Angles angles ) // int modelIndex, Vector3 origin, Angles angles
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

		return ConstructPolygonMesh( model.FirstFace, model.FaceCount, origin, angles );
	}

	private PolygonMesh? ConstructPolygonMesh( int firstFaceIndex, int faceCount, Vector3 origin, Angles angles )
	{
		var geo = Context.Geometry;

		if ( Context.Models is null || geo.Vertices is null || geo.SurfaceEdges is null || geo.EdgeIndices is null || geo.Faces is null || geo.OriginalFaces is null )
		{
			throw new Exception( "No valid map geometry to construct!" );
		}

		var polyMesh = new PolygonMesh();

		// collect all original face indices and their split faces
		var faceCollection = new Dictionary<int, List<int>>();

		var faces = new HashSet<int>();

		for ( int i = 0; i < faceCount; i++ )
		{
			var faceIndex = firstFaceIndex + i;

			var face = geo.Faces[faceIndex];

			// skip faces with invalid area
			if ( face.Area <= 0 || face.Area.AlmostEqual( 0 ) )
				continue;

			var displacementInfoIndex = face.DisplacementInfo;

			// handle displacement faces
			if ( displacementInfoIndex >= 0 )
			{
				if ( geo.DisplacementInfos is null || geo.DisplacementVertices is null )
				{
					Log.Error( $"Displacement face found but no Displacement data present in context! Skipping." );
					continue;
				}

				var dispInfo = geo.DisplacementInfos[displacementInfoIndex];
				ConstructDisplacement( dispInfo );

				// skip displacement base face
				continue;
			}

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
			polyMesh.AddSplitMeshFace( Context, faceIndex, origin );
		}

		//// construct and add faces to poly mesh
		//foreach ( var faceEntry in faceCollection )
		//{
		//	// key is oFaceIndex
		//	var oFaceIndex = faceEntry.Key;

		//	foreach ( var faceIndex in faceEntry.Value )
		//	{
		//		polyMesh.AddSplitMeshFace( Context, faceIndex, origin );
		//	}

		//	//see: PolygonMeshX
		//	//polyMesh.AddOriginalMeshFace( Context, oFaceIndex, origin );
		//}

		// no valid faces in mesh
		if ( !polyMesh.Faces.Any() )
		{
			Log.Error( $"ConstructPolygonMesh failed, [{firstFaceIndex}, {faceCount}] has no valid faces!" );
			return null;
		}

		return polyMesh;
	}

	private void ConstructDisplacement( DisplacementInfo info )
	{
		var geo = Context.Geometry;

		if ( geo.DisplacementInfos is null || geo.DisplacementVertices is null )
		{
			Log.Error( $"Displacement face found but no Displacement data present in context! Skipping." );
			return;
		}

		var power = info.Power;

		var firstVert = info.FirstVertex;
		var vertCount = GetDisplacementVertCount( power );
		var indexCount = vertCount * 2 * 3;
		var triCount = GetDisplacementTriCount( power );

		Log.Info( $"Displacement face with {vertCount} vertices. width: {GetDisplacementWidth( power )} height:{GetDisplacementHeight( power )} " );

		var verts = new Vector3[vertCount];
		var tris = new DispTri[triCount];
		var rIndices = new ushort[indexCount];

		for ( int j = 0; j < vertCount; j++ )
		{
			var dVert = geo.DisplacementVertices[firstVert + j];
			var vert = dVert.Position * dVert.Distance;

			verts[j] = vert;
		}

		for ( int iTri = 0, iRender = 0; iTri < triCount; ++iTri, iRender += 3 )
		{
			//tris[iTri].Indices[0] = rIndices[iRender];
			//tris[iTri].Indices[1] = rIndices[iRender + 1];
			//tris[iTri].Indices[2] = rIndices[iRender + 2];
		}
	}

	private struct DispTri
	{
		public int[] Indices;

		public DispTri( int indices = 3 )
		{
			Indices = new int[indices];
		}
	}

	private int GetDisplacementVertCount( int power )
	{
		return ((1 << power) + 1) * ((1 << power) + 1);
	}

	private int GetDisplacementWidth( int power )
	{
		return (1 << power) + 1;
	}

	private int GetDisplacementHeight( int power )
	{
		return (1 << power) + 1;
	}

	private int GetDisplacementTriCount( int power )
	{
		return (GetDisplacementHeight( power ) - 1) * (GetDisplacementWidth( power ) - 1) * 2;
	}
}
