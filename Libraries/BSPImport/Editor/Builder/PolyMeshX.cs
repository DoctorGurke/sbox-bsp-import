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
		mesh.AddMeshFaceInternal( context, face );
	}
}
