using BspImport.Decompiler;
using BspImport.Decompiler.Lumps;
using Sandbox;
using System;
using System.Diagnostics;
using Tools.MapDoc;
using Tools.MapEditor;

namespace BspImport.Builder;

public class MapBuilder
{
	protected DecompilerContext Context { get; set; }

	public MapBuilder( DecompilerContext context )
	{
		Context = context;
	}

	public void CacheMaterials()
	{
		// materials can only be loaded on the main thread
		ThreadSafe.AssertIsMainThread( $"MapBuilder.CacheMaterials()" );

		Log.Info( $"Caching Materials..." );

		// cache all texData strings as materials
		var splitTexData = $"{Context.TexDataStringData}".Split( ' ' );
		var materialCount = splitTexData.Length;

		for ( int i = 0; i < materialCount; i++ )
		{
			var materialName = splitTexData[i].ToLower();
			var materialPath = $"materials/{materialName}.vmat";
			var material = Material.Load( materialPath );

			if ( material is null )
				continue;

			Context.CachedMaterials.TryAdd( materialName, material );
		}

		Log.Info( $"Done." );
	}

	public void CachePolygonMeshes()
	{
		// caching is always done in parallel
		ThreadSafe.AssertIsNotMainThread();

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

	public void Build()
	{
		// MapEntity and MapMesh can only be created on the main thread
		ThreadSafe.AssertIsMainThread( $"MapBuilder.Build()" );

		BuildEntities();

		BuildGeometry();
	}

	/// <summary>
	/// Build entities parsed from entity lump and static props
	/// </summary>
	protected virtual void BuildEntities()
	{
		ThreadSafe.AssertIsMainThread( $"MapBuilder.Build()" );

		if ( Context.Entities is null )
			return;

		foreach ( var ent in Context.Entities )
		{
			// don't do shit with the worldspawn ent
			if ( ent.ClassName == "worldspawn" )
				continue;

			// brush entities
			if ( ent.Model is not null && ent.Model.StartsWith( '*' ) )
			{
				var modelIndex = int.Parse( ent.Model.TrimStart( '*' ) );
				var polyMesh = Context.CachedPolygonMeshes?[modelIndex];//ConstructModel( modelIndex, ent.Position, ent.Angles );

				if ( polyMesh is null )
				{

					continue;
				}

				var mapMesh = new MapMesh( Hammer.ActiveMap );
				mapMesh.ConstructFromPolygons( polyMesh );
				mapMesh.Name = ent.ClassName;
				mapMesh.Position = ent.Position;
				mapMesh.Angles = ent.Angles;

				continue;
			}

			// regular entity
			var mapent = new MapEntity( Hammer.ActiveMap );
			mapent.ClassName = ent.ClassName;
			mapent.Position = ent.Position;
			mapent.Angles = ent.Angles;
			mapent.Name = ent.GetValue( "targetname" ) ?? "";

			foreach ( var kvp in ent.Data )
			{
				mapent.SetKeyValue( kvp.Key, kvp.Value );
			}
		}
	}

	protected virtual void BuildGeometry()
	{
		ThreadSafe.AssertIsMainThread( $"MapBuilder.Build()" );

		var polyMesh = Context.CachedPolygonMeshes?[0];

		if ( polyMesh is null )
		{
			throw new Exception( $"Tried building WorldSpawn geometry, but Context has no Cached PolygonMeshes!" );
		}

		var mapMesh = new MapMesh( Hammer.ActiveMap );
		mapMesh.ConstructFromPolygons( polyMesh );
		mapMesh.Name = $"worldspawn";
	}

	private PolygonMesh? ConstructModel( int modelIndex, Vector3 origin, Angles angles )
	{
		Log.Info( $"Constructing PolyMesh for {modelIndex}." );

		// if model is already cached, throw
		if ( Context.CachedPolygonMeshes?[modelIndex] is not null )
		{
			throw new Exception( $"Trying to reconstruct already cached model with index: {modelIndex}!" );
		}

		var geo = Context.MapGeometry;

		if ( Context.Models is null || geo.VertexPositions is null || geo.SurfaceEdges is null || geo.EdgeIndices is null || geo.Faces is null || geo.OriginalFaces is null )
		{
			throw new Exception( "No valid map geometry to construct!" );
		}

		if ( modelIndex > Context.Models.Length - 1 )
		{
			throw new Exception( $"Tried to construct map model with index: {modelIndex}. Exceeds available Models!" );
		}

		var model = Context.Models[modelIndex];

		var polyMesh = new PolygonMesh();

		// get original faces from split faces
		var originalfaces = new Dictionary<int, int>();
		for ( int i = 0; i < model.FaceCount; i++ )
		{
			var face = geo.Faces[model.FirstFace + i];

			// no texture info, skip face (SKIP, CLIP, INVISIBLE, etc)
			if ( face.OriginalFaceIndex < 0 || face.TexInfo < 0 )
				continue;

			// we need the texinfo of the split face
			originalfaces.TryAdd( face.OriginalFaceIndex, face.TexInfo );
		}

		// construct original faces
		foreach ( var pair in originalfaces )
		{
			var oFaceIndex = pair.Key;
			var texInfo = pair.Value;
			var oFace = geo.OriginalFaces[oFaceIndex];

			// only construct valid primitives
			if ( oFace.EdgeCount < 2 )
				continue;

			string? material = null;

			// check for valid texinfo, null material falls back to reflectivity 30
			if ( !(texInfo > Context.TexInfo?.Length) )
			{
				material = ParseFaceMaterial( texInfo );
			}

			var verts = new List<Vector3>();

			// get verts from surf edges -> edges -> vertices
			for ( int i = 0; i < oFace.EdgeCount; i++ )
			{
				var edge = geo.SurfaceEdges[oFace.FirstEdge + i];

				// edge sign affects winding order, indexing back to front or vice versa on the edge vertices
				if ( edge >= 0 )
				{
					verts.Add( geo.VertexPositions[geo.EdgeIndices[edge].Indices[0]] );
				}
				else
				{
					verts.Add( geo.VertexPositions[geo.EdgeIndices[-edge].Indices[1]] );
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
				if ( Context.TexInfo?[texInfo] is TexInfo ti )
				{
					var texData = Context.TexData?[ti.TexData];
					if ( texData is TexData t )
					{
						width = t.Width;
						height = t.Height;
					}

					var texCoords = ti.GetUvs( vert + origin, angles, width, height );
					meshVert.TexCoord = texCoords;
				}

				polyMesh.Vertices.Add( meshVert );
				var index = polyMesh.Vertices.Count() - 1;
				indices.Add( index );
			}

			indices.Reverse();

			// get material
			Material? cachedMaterial = null;
			if ( material is not null )
				Context.CachedMaterials.TryGetValue( material, out cachedMaterial );

			// null material falls back to reflectivity 30, so we can just pass it
			var meshFace = new MeshFace( indices, cachedMaterial );
			polyMesh.Faces.Add( meshFace );
		}

		// no valid faces in mesh
		if ( !polyMesh.Faces.Any() )
		{
			Log.Error( $"ConstructModel failed, no valid faces constructed!" );
			return null;
		}

		Log.Info( $"PolyMesh constructed for {modelIndex}." );

		return polyMesh;
	}

	private string? ParseFaceMaterial( int texInfo )
	{
		// get texture/material for face
		var texData = Context.TexInfo?[texInfo].TexData;

		if ( texData is null )
			return null;

		var stringTableIndex = Context.TexDataStringTable?[texData.Value];

		if ( stringTableIndex is null )
			return null;

		return Context.TexDataStringData.FromStringTableIndex( stringTableIndex.Value ).ToLower();
	}
}
