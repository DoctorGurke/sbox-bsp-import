﻿using BspImport;

namespace Sandbox.Builder;

public static class PolyMeshX
{
	private static void AddMeshFaceInternal( this PolygonMesh mesh, ImportContext context, Face face )
	{
		//Log.Info( $"adding face to {mesh} : {mesh.Faces.Count}" );
		var geo = context.Geometry;

		if ( context.Models is null || geo.Vertices is null || geo.SurfaceEdges is null || geo.EdgeIndices is null || geo.Faces is null || geo.OriginalFaces is null )
		{
			return;
		}

		var texInfo = face.TexInfo;

		// only construct valid primitives, 2 edges needed for a triangle
		if ( face.EdgeCount < 2 )
			return;

		string? materialName = null;

		// check for valid texinfo and fetch material name
		if ( !(texInfo > context.TexInfo?.Length) )
		{
			materialName = face.GetFaceMaterial( context );
		}

		if ( materialName!.Contains( "toolsskybox" ) )
			return;

		var verts = new List<Vector3>();
		var uvs = new List<Vector2>();

		// get verts from surf edges -> edges -> vertices
		for ( int i = 0; i < face.EdgeCount; i++ )
		{
			var edge = geo.SurfaceEdges[face.FirstEdge + i];
			Vector3 _vert;

			// edge sign affects winding order, indexing back to front or vice versa on the edge vertices
			if ( edge >= 0 )
			{
				_vert = geo.Vertices[geo.EdgeIndices[edge].Indices[0]];
			}
			else
			{
				_vert = geo.Vertices[geo.EdgeIndices[-edge].Indices[1]];
			}

			verts.Add( _vert );
			uvs.Add( GetTexCoords( context, texInfo, _vert ) );
		}

		verts.Reverse();
		uvs.Reverse();

		// construct mesh vertex from vert pos and calculated uv
		var hVertices = mesh.AddVertices( verts.ToArray() );
		var hFace = mesh.AddFace( hVertices );

		// TODO: uvs
		var material = Material.Load( $"materials/{materialName}.vmat" );
		mesh.SetFaceMaterial( hFace, material );
		mesh.SetFaceTextureCoords( hFace, uvs.ToArray() );
	}

	private static Vector2 GetTexCoords( ImportContext context, int texInfoIndex, Vector3 position, int width = 1024, int height = 1024 )
	{
		// get texture width/height from texdata via texinfo, if available
		if ( context.TexInfo is not null )
		{
			var ti = context.TexInfo[texInfoIndex];

			if ( context.TexData is not null )
			{
				var texData = context.TexData[ti.TexData];
				width = texData.Width;
				height = texData.Height;
			}

			return ti.GetUvs( position, width, height );
		}

		return default;
	}

	//public static void AddOriginalMeshFace( this PolygonMesh mesh, ImportContext context, int oFaceIndex )
	//{
	//	var geo = context.Geometry;

	//	if ( context.Models is null || geo.Vertices is null || geo.SurfaceEdges is null || geo.EdgeIndices is null || geo.Faces is null || geo.OriginalFaces is null )
	//	{
	//		return;
	//	}

	//	var face = geo.OriginalFaces[oFaceIndex];
	//	mesh.AddMeshFaceInternal( context, face );
	//}

	public static void AddSplitMeshFace( this PolygonMesh mesh, ImportContext context, int sFaceIndex )
	{
		var geo = context.Geometry;

		if ( context.Models is null || geo.Vertices is null || geo.SurfaceEdges is null || geo.EdgeIndices is null || geo.Faces is null || geo.OriginalFaces is null )
		{
			return;
		}

		var face = geo.Faces[sFaceIndex];
		mesh.AddMeshFaceInternal( context, face );
	}
}
