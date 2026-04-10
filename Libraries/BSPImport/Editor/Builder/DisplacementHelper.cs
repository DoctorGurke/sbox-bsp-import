using SkiaSharp;
using System;
using System.Collections.Generic;
using System.Text;

namespace BspImport.Builder;

internal class DisplacementHelper
{
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

	public static Vector3? GetDisplacementOrigin( ImportContext context, ushort faceIndex )
	{
		if ( !context.HasCompleteGeometry( out var geo ) )
			return null;

		if ( faceIndex < 0 || faceIndex >= geo.FacesCount )
			return null;

		if ( !geo.TryGetFace( faceIndex, out var face ) )
			return null;

		if ( face.OriginalFaceIndex < 0 || face.OriginalFaceIndex > geo.OriginalFaceCount || !geo.TryGetOriginalFace( face.OriginalFaceIndex, out var oFace ) )
			return null;

		int surfEdgeIdx = oFace.FirstEdge;
		if ( !geo.TryGetSurfaceEdge( surfEdgeIdx, out var edge ) )
			return null;

		int edgeIndex = edge >= 0 ? edge : -edge;
		if ( !geo.TryGetEdgeIndices( edgeIndex, out var edgeIndices ) )
			return null;

		var indices = edgeIndices.Indices;
		if ( indices is null || indices.Length < 2 )
			return null;

		int vertIndex = edge >= 0 ? indices[0] : indices[1];
		if ( !geo.TryGetVertex( vertIndex, out var vertex ) )
			return null;

		return vertex;
	}

	/// <summary>
	/// Creates a displacement Mesh for a face index. Will return null for invalid state. Will Fallback to the base quad if displacement reconstruction fails.
	/// </summary>
	/// <param name="context"></param>
	/// <param name="faceIndex"></param>
	/// <returns>Polygonmesh if any geometry was reconstructed, null for invalid state.</returns>
	public static PolygonMesh? CreateDisplacementMesh( ImportContext context, ushort faceIndex )
	{
		if ( !context.HasCompleteGeometry( out var geo ) )
			return null;

		if ( faceIndex < 0 || faceIndex >= geo.FacesCount )
			return null;

		if ( !geo.TryGetFace( faceIndex, out var face ) )
			return null;

		// passing a non-displacement faceIndex shouldn't fall back, it's wrong usage
		if ( face.DisplacementInfo < 0 )
			return null;

		// we fall back to the base face (quad, I hope) if reconstruction fails along the way
		var mesh = new PolygonMesh();

		// fetch displacement info
		if ( !geo.TryGetDisplacementInfo( face.DisplacementInfo, out var dInfo ) )
		{
			mesh.AddMeshFace( context, face );
			return mesh;
		}

		// original face index should point to a base quad
		if ( face.OriginalFaceIndex < 0 || !geo.TryGetOriginalFace( face.OriginalFaceIndex, out var oFace ) )
		{
			mesh.AddMeshFace( context, face );
			return mesh;
		}

		// gather corner verts from original face
		var corners = new List<Vector3>();
		for ( int i = 0; i < oFace.EdgeCount; i++ )
		{
			int surfEdgeIdx = oFace.FirstEdge + i;
			if ( !geo.TryGetSurfaceEdge( surfEdgeIdx, out var edge ) )
			{
				mesh.AddMeshFace( context, face );
				return mesh;
			}

			int edgeIndex = edge >= 0 ? edge : -edge;
			if ( !geo.TryGetEdgeIndices( edgeIndex, out var edgeIndices ) )
			{
				mesh.AddMeshFace( context, face );
				return mesh;
			}

			var indices = edgeIndices.Indices;
			if ( indices is null || indices.Length < 2 )
			{
				mesh.AddMeshFace( context, face );
				return mesh;
			}

			int vertIndex = edge >= 0 ? indices[0] : indices[1];
			if ( !geo.TryGetVertex( vertIndex, out var vertex ) )
			{
				mesh.AddMeshFace( context, face );
				return mesh;
			}

			corners.Add( vertex );
		}

		// we expect a quad base
		if ( corners.Count != 4 )
		{
			mesh.AddMeshFace( context, face );
			return mesh;
		}

		// resolve material for displacement face
		string? materialName = null;
		Vector3 reflectivity = Vector3.One;
		if ( context.TexInfo is not null && face.TexInfo >= 0 && face.TexInfo < context.TexInfo.Length )
		{
			materialName = face.GetMaterialName( context );
			reflectivity = face.GetReflectivity( context );
		}

		// load material for displacement triangles if we have a name
		Material? dispMaterial = null;
		if ( !string.IsNullOrEmpty( materialName ) && context.BuildSettings.LoadMaterials )
		{
			dispMaterial = Material.Load( $"materials/{materialName}.vmat" );
		}

		// start gathering everything required, orient corners
		int pointStartIndex = FindClosestCorner( corners.ToArray(), dInfo.StartPosition );
		var rotatedCorners = RotateCornerArray( corners.ToArray(), pointStartIndex );

		int power = dInfo.Power;
		int side = dInfo.Side;
		int count = dInfo.VertCount;

		var positions = new Vector3[count];
		var uvs = new Vector2[count];

		// Read displacement vertices in storage order (X-major: x*side + y)
		var storedVerts = new DisplacementVertex[count];
		for ( int sx = 0; sx < side; sx++ )
		{
			for ( int sy = 0; sy < side; sy++ )
			{
				int dvIndex = dInfo.FirstVertex + sx * side + sy;
				if ( !geo.TryGetDisplacementVertex( dvIndex, out var dVert ) )
				{
					mesh.AddMeshFace( context, face );
					return mesh;
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

		// Populate positions/uvs
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
				uvs[idx] = MapBuilder.GetTexCoords( context, face.TexInfo, finalPos );
			}
		}

		if ( positions.Length > dInfo.VertCount )
		{
			return mesh;
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

				// quad vertices
				var v_a = positions[a];
				var v_b = positions[b];
				var v_c = positions[c];
				var v_d = positions[d];

				// quad uvs
				var uv_a = uvs[a];
				var uv_b = uvs[b];
				var uv_c = uvs[c];
				var uv_d = uvs[d];

				//var faceVerts = new[] { hVerts[a], hVerts[c], hVerts[d], hVerts[b] };
				//var faceUvs = new[] { uvs[a], uvs[c], uvs[d], uvs[b] };
				//var hFace = mesh.AddFace( faceVerts );
				//mesh.SetFaceTextureCoords( hFace, faceUvs );
				//continue;

				// just use Triangle for area check
				var _tri1 = new Triangle( v_a, v_c, v_b );
				var _tri2 = new Triangle( v_c, v_d, v_b );

				const float epsilon = 1e-6f;
				// skip degenerate faces
				if ( _tri1.Area < epsilon || _tri2.Area < epsilon )
				{
					continue;
				}

				// per-tri vertices - winding
				var v_t1 = new[] { hVerts[a], hVerts[c], hVerts[b] };
				var v_t2 = new[] { hVerts[c], hVerts[d], hVerts[b] };


				// per-tri uvs
				var uv_t1 = new[] { uv_a, uv_c, uv_b };
				var uv_t2 = new[] { uv_c, uv_d, uv_b };

				var materialFallback = $"bsp_vertex_color";

				var t1 = mesh.AddFace( v_t1 );
				//mesh.SetEdgeSmoothing( t1.Edge, PolygonMesh.EdgeSmoothMode.Soft );
				mesh.SetFaceTextureCoords( t1, uv_t1 );
				if ( dispMaterial is not null )
				{
					mesh.SetFaceMaterial( t1, dispMaterial );
				}
				else
				{
					var material = Material.Load( $"materials/{materialFallback}.vmat" );
					mesh.SetFaceMaterial( t1, material );

					foreach ( var edge in mesh.HalfEdgeHandles )
					{
						Color col = new Color( reflectivity.x, reflectivity.y, reflectivity.z );
						mesh.SetVertexColor( edge, col.ToColor32( true ) );
					}
				}

				var t2 = mesh.AddFace( v_t2 );
				//mesh.SetEdgeSmoothing( t2.Edge, PolygonMesh.EdgeSmoothMode.Soft );
				mesh.SetFaceTextureCoords( t2, uv_t2 );
				if ( dispMaterial is not null )
				{
					mesh.SetFaceMaterial( t2, dispMaterial );
				}
				else
				{
					var material = Material.Load( $"materials/{materialFallback}.vmat" );
					mesh.SetFaceMaterial( t2, material );

					foreach ( var edge in mesh.HalfEdgeHandles )
					{
						Color col = new Color( reflectivity.x, reflectivity.y, reflectivity.z );
						mesh.SetVertexColor( edge, col.ToColor32( true ) );
					}
				}
			}
		}

		return mesh;
	}
}
