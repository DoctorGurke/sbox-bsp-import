using BspImport;

namespace Sandbox.Builder;

public static class PolyMeshX
{
	private static void AddMeshFaceInternal( this PolygonMesh mesh, ImportContext context, Face face )
	{
		//Log.Info( $"adding face to {mesh} : {mesh.Faces.Count}" );
		MapGeometry geo;

		if ( !context.HasCompleteGeometry( out geo ) )
		{
			return;
		}

		var texInfo = face.TexInfo;

		// only construct valid primitives, 2 edges needed for a triangle
		if ( face.EdgeCount < 2 )
			return;

		// validate surface edge range
		if ( face.FirstEdge < 0 || face.FirstEdge >= geo.SurfaceEdgesCount || face.FirstEdge + face.EdgeCount > geo.SurfaceEdgesCount )
			return;

		string? materialName = null;

		// check for valid texinfo and fetch material name
		if ( context.TexInfo is not null && texInfo >= 0 && texInfo < context.TexInfo.Length )
		{
			materialName = face.GetFaceMaterial( context );
		}

		if ( string.IsNullOrEmpty( materialName ) )
			return;

		var isToolsMaterial = materialName.Contains( "tools", StringComparison.OrdinalIgnoreCase );

		if ( !context.Settings.ImportToolMaterials && isToolsMaterial )
			return;

		var verts = new List<Vector3>();
		var uvs = new List<Vector2>();

		// get verts from surf edges -> edges -> vertices
		for ( int i = 0; i < face.EdgeCount; i++ )
		{
			int surfEdgeIdx = face.FirstEdge + i;
			if ( !geo.TryGetSurfaceEdge( surfEdgeIdx, out var edge ) )
				return;

			// edge sign affects winding order, indexing back to front or vice versa on the edge vertices
			int edgeIndex = edge >= 0 ? edge : -edge;
			if ( !geo.TryGetEdgeIndices( edgeIndex, out var edgeIndices ) )
				return;

			var indices = edgeIndices.Indices;
			if ( indices is null || indices.Length < 2 )
				return;

			int vertIdx = edge >= 0 ? indices[0] : indices[1];
			if ( !geo.TryGetVertex( vertIdx, out var vertex ) )
				return;

			verts.Add( vertex );
			uvs.Add( GetTexCoords( context, texInfo, vertex ) );
		}

		verts.Reverse();
		uvs.Reverse();

		// construct mesh vertex from vert pos and calculated uv
		var hVertices = mesh.AddVertices( verts.ToArray() );
		var hFace = mesh.AddFace( hVertices );

		if ( context.Settings.LoadMaterials )
		{
			var material = Material.Load( $"materials/{materialName}.vmat" );
			mesh.SetFaceMaterial( hFace, material );
		}

		if ( isToolsMaterial )
			mesh.TextureAlignToGrid( Transform.Zero );
		else
			mesh.SetFaceTextureCoords( hFace, uvs.ToArray() );
	}

	private static Vector2 GetTexCoords( ImportContext context, int texInfoIndex, Vector3 position, int width = 1024, int height = 1024 )
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

	public static void AddSplitMeshFace( this PolygonMesh mesh, ImportContext context, int sFaceIndex )
	{
		if ( !context.HasCompleteGeometry( out var geo ) )
			return;

		if ( sFaceIndex < 0 || sFaceIndex >= geo.FacesCount )
			return;

		geo.TryGetFace( sFaceIndex, out var face );

		// skip displacement faces
		if ( face.DisplacementInfo >= 0 )
		{
			return;
		}
		else
		{
			mesh.AddMeshFaceInternal( context, face );
		}
	}

	private static int FindClosestCorner( Vector3[] corners, Vector3 startPosition )
	{
		int minIndex = -1;
		float minDistance = float.MaxValue;

		for ( int i = 0; i < 4; i++ )
		{
			Vector3 segment = startPosition - corners[i];
			float distanceSq = segment.LengthSquared;
			if ( distanceSq < minDistance )
			{
				minDistance = distanceSq;
				minIndex = i;
			}
		}

		return minIndex;
	}

	private static Vector3[] RotateCornerArray( Vector3[] corners, int pointStartIndex )
	{
		var rotatedCorners = new Vector3[4];

		for ( int i = 0; i < 4; i++ )
		{
			rotatedCorners[i] = corners[(i + pointStartIndex) % 4];
		}

		return rotatedCorners;
	}

	public static void AddDisplacementMesh( this PolygonMesh mesh, ImportContext context, ushort faceIndex )
	{
		if ( !context.HasCompleteGeometry( out var geo ) )
			return;

		if ( faceIndex < 0 || faceIndex >= geo.FacesCount )
			return;

		geo.TryGetFace( faceIndex, out var face );

		if ( face.DisplacementInfo < 0 )
			return;

		geo.TryGetDisplacementInfo( face.DisplacementInfo, out var dispInfo );


		// resolve material for displacement face
		string? materialName = null;
		if ( context.TexInfo is not null && face.TexInfo >= 0 && face.TexInfo < context.TexInfo.Length )
		{
			materialName = face.GetFaceMaterial( context );
		}

		// load material for displacement triangles if we have a name
		Material? dispMaterial = null;
		if ( !string.IsNullOrEmpty( materialName ) )
		{
			dispMaterial = Material.Load( $"materials/{materialName}.vmat" );
		}

		// fetch displacement info
		if ( !geo.TryGetDisplacementInfo( face.DisplacementInfo, out var dInfo ) )
		{
			// fallback to normal face handling
			mesh.AddMeshFaceInternal( context, face );
			return;
		}

		// original face index should point to a base quad
		if ( face.OriginalFaceIndex < 0 || !geo.TryGetOriginalFace( face.OriginalFaceIndex, out var oFace ) )
		{
			mesh.AddMeshFaceInternal( context, face );
			return;
		}

		// gather corner verts from original face
		var corners = new List<Vector3>();
		for ( int i = 0; i < oFace.EdgeCount; i++ )
		{
			int surfEdgeIdx = oFace.FirstEdge + i;
			if ( !geo.TryGetSurfaceEdge( surfEdgeIdx, out var edge ) )
			{
				mesh.AddMeshFaceInternal( context, face );
				return;
			}

			int edgeIndex = edge >= 0 ? edge : -edge;
			if ( !geo.TryGetEdgeIndices( edgeIndex, out var edgeIndices ) )
			{
				mesh.AddMeshFaceInternal( context, face );
				return;
			}

			var indices = edgeIndices.Indices;
			if ( indices is null || indices.Length < 2 )
			{
				mesh.AddMeshFaceInternal( context, face );
				return;
			}

			int vertIdx = edge >= 0 ? indices[0] : indices[1];
			if ( !geo.TryGetVertex( vertIdx, out var vertex ) )
			{
				mesh.AddMeshFaceInternal( context, face );
				return;
			}

			corners.Add( vertex );
		}

		// we expect a quad base; if not, fallback
		if ( corners.Count != 4 )
		{
			mesh.AddMeshFaceInternal( context, face );
			return;
		}

		int pointStartIndex = FindClosestCorner( corners.ToArray(), dInfo.StartPosition );
		var rotatedCorners = RotateCornerArray( corners.ToArray(), pointStartIndex );

		int power = dInfo.Power;
		int side = dInfo.Side;
		int count = dInfo.VertCount;

		var positions = new Vector3[count];
		var uvs = new Vector2[count];

		// Read displacement vertices in storage order (X-major: x*side + y)
		// Use flipped Y when reading to correct mirroring in many BSPs while preserving X-major ordering.
		var storedVerts = new DisplacementVertex[count];
		for ( int sx = 0; sx < side; sx++ )
		{
			for ( int sy = 0; sy < side; sy++ )
			{
				int dvIndex = dInfo.FirstVertex + sx * side + sy;
				if ( !geo.TryGetDisplacementVertex( dvIndex, out var dVert ) )
				{
					mesh.AddMeshFaceInternal( context, face );
					return;
				}

				storedVerts[sx * side + sy] = dVert;
			}
		}

		// Build base grid positions (without displacement) for orientation matching
		var baseGrid = new Vector3[count];
		for ( int bx = 0; bx < side; bx++ )
		{
			for ( int by = 0; by < side; by++ )
			{
				float s = (float)bx / (side - 1);
				float t = (float)by / (side - 1);
				var bottom = Vector3.Lerp( rotatedCorners[0], rotatedCorners[1], s );
				var top = Vector3.Lerp( rotatedCorners[3], rotatedCorners[2], s );
				baseGrid[bx * side + by] = Vector3.Lerp( bottom, top, t );
			}
		}

		// Populate positions/uvs by rotating the storage grid into the base grid orientation.
		// Then rotate all displacement positions 90 degrees around world Z (up) about the face center.
		for ( int sx = 0; sx < side; sx++ )
		{
			for ( int sy = 0; sy < side; sy++ )
			{
				var dVert = storedVerts[sx * side + sy];

				float s = side <= 1 ? 0f : (float)sx / (side - 1);
				float t = side <= 1 ? 0f : (float)sy / (side - 1);

				var bottom = Vector3.Lerp( rotatedCorners[0], rotatedCorners[1], s );
				var top = Vector3.Lerp( rotatedCorners[3], rotatedCorners[2], s );
				var basePos = Vector3.Lerp( bottom, top, t );

				var finalPos = basePos + dVert.Displacement * dVert.Distance;

				int idx = sy * side + sx; // base grid is row-major (y * side + x)
				positions[idx] = finalPos;
				uvs[idx] = GetTexCoords( context, face.TexInfo, finalPos );
			}
		}

		var hVerts = mesh.AddVertices( positions.ToArray() );

		// triangulate quads into triangles and add as faces
		for ( int y = 0; y < side - 1; y++ )
		{
			for ( int x = 0; x < side - 1; x++ )
			{
				int a = y * side + x;
				int b = a + 1;
				int c = a + side;
				int d = c + 1;

				// two tris: (a, b, c) and (b, d, c) — ordering chosen to match face winding
				var t1 = mesh.AddFace( new[] { hVerts[a], hVerts[c], hVerts[b] } );
				mesh.SetEdgeSmoothing( t1.Edge, PolygonMesh.EdgeSmoothMode.Soft );
				mesh.SetFaceTextureCoords( t1, new[] { uvs[a], uvs[c], uvs[b] } );
				if ( dispMaterial is not null && context.Settings.LoadMaterials ) mesh.SetFaceMaterial( t1, dispMaterial );

				var t2 = mesh.AddFace( new[] { hVerts[c], hVerts[d], hVerts[b] } );
				mesh.SetEdgeSmoothing( t2.Edge, PolygonMesh.EdgeSmoothMode.Soft );
				mesh.SetFaceTextureCoords( t2, new[] { uvs[c], uvs[d], uvs[b] } );
				if ( dispMaterial is not null && context.Settings.LoadMaterials ) mesh.SetFaceMaterial( t2, dispMaterial );
			}
		}
	}
}
