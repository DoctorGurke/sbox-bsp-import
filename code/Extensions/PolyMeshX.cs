﻿namespace BspImport.Extensions;

public static class PolyMeshX
{
	private static void AddMeshFaceInternal( this PolygonMesh mesh, ImportContext context, Face face, Vector3 origin )
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

		var verts = new List<Vector3>();

		// get verts from surf edges -> edges -> vertices
		for ( int i = 0; i < face.EdgeCount; i++ )
		{
			var edge = geo.SurfaceEdges[face.FirstEdge + i];

			// edge sign affects winding order, indexing back to front or vice versa on the edge vertices
			if ( edge >= 0 )
			{
				verts.Add( geo.Vertices[geo.EdgeIndices[edge].Indices[0]] );
			}
			else
			{
				verts.Add( geo.Vertices[geo.EdgeIndices[-edge].Indices[1]] );
			}
		}

		// construct mesh vertex from vert pos and calculated uv
		var indices = new List<int>();
		foreach ( var vert in verts )
		{
			var meshVert = new MeshVertex();
			meshVert.Position = vert;

			var width = 1024;
			var height = 1024;

			// get texture width/height from texdata via texinfo
			if ( context.TexInfo?[texInfo] is TexInfo ti )
			{
				var texData = context.TexData?[ti.TexData];
				if ( texData is TexData t )
				{
					width = t.Width;
					height = t.Height;
				}

				// construct uvs
				var texCoords = ti.GetUvs( vert, width, height );
				meshVert.TexCoord = texCoords;
			}

			mesh.Vertices.Add( meshVert );
			var index = mesh.Vertices.Count() - 1;
			indices.Add( index );
		}

		// inverse winding
		indices.Reverse();

		// get material
		var material = Material.Load( $"materials/{materialName}.vmat" );

		var meshFace = new MeshFace( indices, material );

		mesh.Faces.Add( meshFace );
	}

	public static void AddOriginalMeshFace( this PolygonMesh mesh, ImportContext context, int oFaceIndex, Vector3 origin )
	{
		var geo = context.Geometry;

		if ( context.Models is null || geo.Vertices is null || geo.SurfaceEdges is null || geo.EdgeIndices is null || geo.Faces is null || geo.OriginalFaces is null )
		{
			return;
		}

		var face = geo.OriginalFaces[oFaceIndex];
		mesh.AddMeshFaceInternal( context, face, origin );
	}

	public static void AddSplitMeshFace( this PolygonMesh mesh, ImportContext context, int sFaceIndex, Vector3 origin )
	{
		var geo = context.Geometry;

		if ( context.Models is null || geo.Vertices is null || geo.SurfaceEdges is null || geo.EdgeIndices is null || geo.Faces is null || geo.OriginalFaces is null )
		{
			return;
		}

		var face = geo.Faces[sFaceIndex];
		mesh.AddMeshFaceInternal( context, face, origin );
	}
}
