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
		{
			Log.Warning( $"skipped degenerate face!" );
			return;
		}

		// validate surface edge range
		if ( face.FirstEdge < 0 || face.FirstEdge >= geo.SurfaceEdgesCount || face.FirstEdge + face.EdgeCount > geo.SurfaceEdgesCount )
		{
			Log.Warning( $"skipped face with invalid surface edge range!" );
			return;
		}

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
		if ( context.BuildSettings.CullSkybox && materialName.Contains( "toolsskybox" ) )
			return;

		var verts = face.GetEdgeVertices( context );
		if ( verts is null )
			return;

		var uvs = verts.Select( v => MapBuilder.GetTexCoords( context, texInfo, v ) ).ToList();

		verts.Reverse();
		uvs.Reverse();

		// construct mesh vertex from vert pos and calculated uv
		var hVertices = mesh.AddVertices( verts.ToArray() );
		var hFace = mesh.AddFace( hVertices );

		if ( context.BuildSettings.LoadMaterials )
		{
			var material = Material.Load( $"materials/{materialName}.vmat" );
			mesh.SetFaceMaterial( hFace, material );
		}
		else
		{
			var materialFallback = $"bsp_vertex_color";

			if ( isWater )
			{
				materialFallback = "bsp_water";
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

	public static void AddMeshFace( this PolygonMesh mesh, ImportContext context, Face face )
	{
		mesh.AddMeshFaceInternal( context, face );
	}
}
