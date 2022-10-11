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

	public void Build()
	{
		BuildEntities();

		BuildGeometry();
	}

	/// <summary>
	/// Build entities parsed from entity lump and static props
	/// </summary>
	protected virtual void BuildEntities()
	{
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
				var index = int.Parse( ent.Model.TrimStart( '*' ) );
				var polyMesh = ConstructModel( index, ent.Position, ent.Angles );

				var mapMesh = new MapMesh( Hammer.ActiveMap );
				mapMesh.ConstructFromPolygons( polyMesh );
				mapMesh.Name = ent.ClassName;

				continue;
			}

			var mapent = new MapEntity( Hammer.ActiveMap );
			mapent.ClassName = ent.ClassName;
			mapent.Position = ent.Position;
			mapent.Angles = ent.Angles;

			foreach ( var kvp in ent.Data )
			{
				mapent.SetKeyValue( kvp.Key, kvp.Value );
			}
		}
	}

	protected virtual void BuildGeometry()
	{
		var polyMesh = Context.WorldSpawn;

		if ( polyMesh is null )
		{
			Log.Error( $"Tried building world geometry without a valid WorldSpawn present in the current DecompilerContext!" );
			return;
		}

		var mapMesh = new MapMesh( Hammer.ActiveMap );
		mapMesh.ConstructFromPolygons( polyMesh );
		mapMesh.Name = $"worldspawn";
	}

	public void PrepareWorldSpawn()
	{
		Context.PreparedWorldSpawn = false;
		Context.WorldSpawn = ConstructModel( 0 );
		Context.PreparedWorldSpawn = true;
	}

	private RealTimeSince TimeSinceUpdate { get; set; }

	private PolygonMesh? ConstructModel( int index ) => ConstructModel( index, Vector3.Zero, Angles.Zero );

	private PolygonMesh? ConstructModel( int index, Vector3 origin, Angles angles )
	{
		var geo = Context.MapGeometry;

		if ( Context.Models is null || geo.VertexPositions is null || geo.SurfaceEdges is null || geo.EdgeIndices is null || geo.Faces is null || geo.OriginalFaces is null )
		{
			throw new Exception( "No valid map geometry to construct!" );
		}

		if ( index > Context.Models.Count() - 1 )
		{
			throw new Exception( $"Tried to construct map model with index: {index}. Exceeds available Models!" );
		}

		var model = Context.Models.ElementAt( index );

		var polyMesh = new PolygonMesh();

		// get original faces from split faces
		var originalfaces = new Dictionary<int, int>();
		for ( int i = 0; i < model.FaceCount; i++ )
		{
			var face = geo.Faces.ElementAt( model.FirstFace + i );

			// no texture info, skip face (SKIP, CLIP, INVISIBLE, etc)
			if ( face.TexInfo == -1 )
				continue;

			// we need the texinfo of the split face
			originalfaces.TryAdd( face.OriginalFaceIndex, face.TexInfo );
		}

		var total = originalfaces.Count();
		var current = 0;

		Log.Info( $"ORIGINAL FACES: {total}" );

		var time = new Stopwatch();

		// construct original faces
		foreach ( var pair in originalfaces )
		{
			time.Restart();

			var oFaceIndex = pair.Key;
			var texInfo = pair.Value;
			var oFace = geo.OriginalFaces.ElementAt( oFaceIndex );

			// only construct valid primitives
			if ( oFace.EdgeCount < 3 )
				continue;

			// if split face texinfo is invalid, try it with original face texinfo?
			if ( texInfo > Context.TexInfo?.Count() )
			{
				//texinfo = face.TexInfo;
				Log.Info( $"invalid texinfo: {texInfo} {oFace.TexInfo}" );
			}

			string? material;

			// still invalid, just use default material
			if ( texInfo > Context.TexInfo?.Count() )
			{
				//Log.Info( $"skipping face with invalid texinfo: {texinfo}" );
				material = null;
			}
			else
			{
				material = ParseFaceMaterial( texInfo );
			}

			// fall back to default material if parsing failed
			if ( material is null )
			{
				material = $"materials/dev/reflectivity_30.vmat";
			}

			// time elapsed to parse material data
			var materialTime = time.ElapsedMilliseconds;

			var verts = new List<Vector3>();

			// get verts from surf edges -> edges -> vertices
			for ( int i = 0; i < oFace.EdgeCount; i++ )
			{
				var edge = geo.SurfaceEdges.ElementAt( oFace.FirstEdge + i );

				// edge sign affects winding order, indexing back to front or vice versa on the edge vertices
				if ( edge >= 0 )
				{
					verts.Add( geo.VertexPositions.ElementAt( geo.EdgeIndices.ElementAt( edge ).Indices[0] ) );
				}
				else
				{
					verts.Add( geo.VertexPositions.ElementAt( geo.EdgeIndices.ElementAt( -edge ).Indices[1] ) );
				}
			}

			// time elapsed to fetch verts
			var vertsTime = time.ElapsedMilliseconds - materialTime;

			// construct mesh vertex from vert pos and calculated uv
			var indices = new List<int>();
			foreach ( var vert in verts )
			{
				var meshVert = new MeshVertex();
				meshVert.Position = vert + origin;

				var width = 1024;
				var height = 1024;

				// get texture width/height from texdata via texinfo
				if ( Context.TexInfo?.ElementAt( texInfo ) is TexInfo ti )
				{
					var texData = Context.TexData?.ElementAt( ti.TexData );
					if ( texData is TexData t )
					{
						width = t.Width;
						height = t.Height;
					}

					var texCoords = ti.GetUvs( vert, angles, width, height );
					meshVert.TexCoord = texCoords;
				}

				polyMesh.Vertices.Add( meshVert );
				indices.Add( polyMesh.Vertices.Count() - 1 );
			}

			indices.Reverse();

			// time elapsed to construct meshvertices and fetch indices
			var indicesTime = time.ElapsedMilliseconds - (vertsTime + materialTime);

			var meshFace = new MeshFace( indices, null ); //Material.Load( material )
			polyMesh.Faces.Add( meshFace );

			// time elapsed to construct meshfaces
			var meshFaceTime = time.ElapsedMilliseconds - (indicesTime + vertsTime + materialTime);

			if ( TimeSinceUpdate >= 1.0f )
			{
				Log.Info( $"##### Original faces built: {current} / {total} : {(current / total) * 100.0f}%" );

				TimeSinceUpdate = 0;
			}

			Log.Info( $"@Original Face Took: {time.ElapsedMilliseconds}ms | material time: {materialTime}ms verts time: {vertsTime}ms indices time: {indicesTime}ms meshFace time: {meshFaceTime}ms" );
			time.Reset();

			current++;
		}

		// no valid faces in mesh
		if ( !polyMesh.Faces.Any() )
		{
			Log.Error( $"ConstructModel failed! No valid faces constructed!" );
			return null;
		}

		Log.Info( $"PolyMesh constructed." );

		return polyMesh;
	}

	private string? ParseFaceMaterial( int texInfo )
	{
		// get texture/material for face
		var texData = Context.TexInfo?.ElementAtOrDefault( texInfo ).TexData;

		if ( texData is null )
			return null;

		var stringTableIndex = Context.TexDataStringTable?.ElementAtOrDefault( texData.Value );

		if ( stringTableIndex is null )
			return null;

		return $"materials/{Context.TexDataStringData.FromStringTableIndex( stringTableIndex.Value ).ToLower()}.vmat";
	}
}
