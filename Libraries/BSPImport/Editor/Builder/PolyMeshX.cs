using BspImport;
using HalfEdgeMesh;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace BspImport.Builder;

public static class PolyMeshX
{
	private static void AddMeshFaceInternal( this PolygonMesh mesh, ImportContext context, Face face )
	{
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

		Vector3 reflectivity = Vector3.One;

		bool isWater = false;

		// check for valid texinfo and fetch material name
		if ( context.TexInfo is not null && texInfo >= 0 && texInfo < context.TexInfo.Length )
		{
			materialName = face.GetMaterialName( context );
			reflectivity = face.GetReflectivity( context );

			var surfaceFlags = face.GetSurfaceFlags( context );
			if ( (surfaceFlags & SurfaceFlags.Warp) != 0 )
			{
				isWater = true;
			}
		}

		if ( string.IsNullOrEmpty( materialName ) )
			return;

		// simple patch
		materialName = materialName.Replace( "toolsskybox2d", "toolsskybox" );

		// cull skybox faces entirely
		if ( context.Settings.CullSkybox && materialName.Contains( "toolsskybox" ) )
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
			uvs.Add( MapBuilder.GetTexCoords( context, texInfo, vertex ) );
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
		else
		{
			var materialFallback = $"test_vertex_color";

			if ( isWater )
			{
				materialFallback = "test_water";
			}

			var material = Material.Load( $"materials/{materialFallback}.vmat" );
			mesh.SetFaceMaterial( hFace, material );

			foreach ( var edge in mesh.HalfEdgeHandles )
			{
				Color col = new Color( reflectivity.x, reflectivity.y, reflectivity.z );
				mesh.SetVertexColor( edge, col.ToColor32( true ) );
			}
		}

		// uv fix for tools materials
		if ( materialName.Contains( "tools" ) )
			mesh.TextureAlignToGrid( Transform.Zero );
		else
			mesh.SetFaceTextureCoords( hFace, uvs.ToArray() );
	}

	public static void AddMeshFace( this PolygonMesh mesh, ImportContext context, ushort faceIndex )
	{
		if ( !context.HasCompleteGeometry( out var geo ) )
			return;

		if ( faceIndex < 0 || faceIndex >= geo.FacesCount )
			return;

		if ( !geo.TryGetFace( faceIndex, out var face ) )
			return;

		mesh.AddMeshFaceInternal( context, face );
	}
}
