using BspImport;
using System;

namespace Sandbox.Builder;

public static class PolyMeshX
{
	private static void AddMeshFaceInternal( this PolygonMesh mesh, ImportContext context, Face face )
	{
		//Log.Info( $"adding face to {mesh} : {mesh.Faces.Count}" );
		MapGeometry geo;

		if ( !context.HasCompleteGeometry(out geo) )
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
		if (context.TexInfo is not null && texInfo >= 0 && texInfo < context.TexInfo.Length)
		{
			materialName = face.GetFaceMaterial( context );
		}

		if ( string.IsNullOrEmpty( materialName ) )
			return;

		// TODO: settings
		if ( materialName.Contains( "toolsskybox", StringComparison.OrdinalIgnoreCase ) )
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

		//var material = Material.Load( $"materials/{materialName}.vmat" );
		//mesh.SetFaceMaterial( hFace, material );
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
		// handle displacement faces by reconstructing their triangulated quad mesh
		if ( face.DisplacementInfo >= 0 )
		{
          // resolve material for displacement face
			string? materialName = null;
			if (context.TexInfo is not null && face.TexInfo >= 0 && face.TexInfo < context.TexInfo.Length)
			{
				materialName = face.GetFaceMaterial(context);
			}

            // If we don't have a material name, still construct the displacement geometry
			// but don't attempt to load a material. Skip only skybox materials.
			if ( !string.IsNullOrEmpty(materialName) )
			{
				if ( materialName.Contains( "toolsskybox", StringComparison.OrdinalIgnoreCase ) )
				{
					return;
				}
			}

			// load material for displacement triangles if we have a name
			Material? dispMaterial = null;
			if ( !string.IsNullOrEmpty(materialName) )
			{
				try
				{
					dispMaterial = Material.Load( $"materials/{materialName}.vmat" );
				}
				catch
				{
					dispMaterial = null;
				}
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

			// match ordering used for regular faces
			corners.Reverse();

			int power = dInfo.Power;
			int side = (1 << power) + 1;
			int count = side * side;

			var positions = new Vector3[count];
			var uvs = new Vector2[count];

			// populate grid
			for ( int y = 0; y < side; y++ )
			{
				for ( int x = 0; x < side; x++ )
				{
					float s = side <= 1 ? 0f : (float)x / (side - 1);
					float t = side <= 1 ? 0f : (float)y / (side - 1);

					var bottom = Vector3.Lerp( corners[0], corners[1], s );
					var top = Vector3.Lerp( corners[3], corners[2], s );
					var basePos = Vector3.Lerp( bottom, top, t );

					int dvIndex = dInfo.FirstVertex + y * side + x;
					if ( !geo.TryGetDisplacementVertex( dvIndex, out var dVert ) )
					{
						mesh.AddMeshFaceInternal( context, face );
						return;
					}

					var finalPos = basePos + dVert.Displacement * dVert.Distance;
					int idx = y * side + x;
					positions[idx] = finalPos;
					uvs[idx] = GetTexCoords( context, face.TexInfo, finalPos );
				}
			}

            // do not reverse grid ordering – keep row-major (left-to-right, top-to-bottom)

           // add all grid vertices once
			var hVerts = mesh.AddVertices( positions.ToArray() );

			Log.Info($"disp hVerts count={hVerts?.Length ?? 0}");
			if ( hVerts is not null && hVerts.Length > 0 )
			{
				Log.Info($"disp hVerts sample: first={hVerts[0]} last={hVerts[hVerts.Length - 1]}");
			}

			foreach (var vert in positions )
			{
				Log.Info($"disp vert: {vert}");
				var sphere = new Sphere( vert, 1 );
				DebugOverlaySystem.Get( SceneEditorSession.Active.Scene ).Sphere(sphere, duration: 30, overlay: true);
			}

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
                    var t1 = mesh.AddFace( new[] { hVerts[a], hVerts[b], hVerts[c] } );
					Log.Info($"added disp tri t1={t1} indices={a},{b},{c}");
					mesh.SetFaceTextureCoords( t1, new[] { uvs[a], uvs[b], uvs[c] } );
					if ( dispMaterial is not null ) mesh.SetFaceMaterial( t1, dispMaterial );

                    var t2 = mesh.AddFace( new[] { hVerts[b], hVerts[d], hVerts[c] } );
					Log.Info($"added disp tri t2={t2} indices={b},{d},{c}");
					mesh.SetFaceTextureCoords( t2, new[] { uvs[b], uvs[d], uvs[c] } );
					if ( dispMaterial is not null ) mesh.SetFaceMaterial( t2, dispMaterial );
				}
			}

			Log.Info($"added displacement mesh: faces={mesh.FaceHandles.Count()}, verts={mesh.VertexHandles.Count()}");

			return;
		}
		else
		{
			mesh.AddMeshFaceInternal( context, face );
		}
	}
}
