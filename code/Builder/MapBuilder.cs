using BspImport.Decompiler;
using Sandbox;
using System.Linq;
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
				ConstructModel( index, ent.Position, ent.ClassName );
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
		ConstructModel( 0, "worldspawn" );
	}

	private MapMesh? ConstructModel( int index, string? name = null ) => ConstructModel( index, Vector3.Zero, name );

	private MapMesh? ConstructModel( int index, Vector3 origin, string? name = null )
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

		// construct original faces
		foreach ( var pair in originalfaces )
		{
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

			// construct mesh vertex from vert pos (temp until we have texinfo uv)
			var indices = new List<int>();
			foreach ( var vert in verts )
			{
				var meshVert = new MeshVertex();
				meshVert.Position = vert + origin;
				polyMesh.Vertices.Add( meshVert );
				indices.Add( polyMesh.Vertices.Count() - 1 );
			}

			indices.Reverse();

			var meshFace = new MeshFace( indices, Material.Load( material ) );
			polyMesh.Faces.Add( meshFace );
		}

		// no valid faces in mesh
		if ( !polyMesh.Faces.Any() )
		{
			Log.Error( $"ConstructModel failed! No valid faces constructed!" );
			return null;
		}

		var mapMesh = new MapMesh( Hammer.ActiveMap );
		mapMesh.ConstructFromPolygons( polyMesh );
		mapMesh.Name = name;

		return mapMesh;
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
