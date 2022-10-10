using BspImport.Decompiler;
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

		var polymesh = new PolygonMesh();

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
			var origface = pair.Key;
			var texinfo = pair.Value;
			var face = geo.OriginalFaces.ElementAt( origface );

			// only construct valid primitives
			if ( face.EdgeCount < 3 )
				continue;

			// if split face texinfo is invalid, try it with original face texinfo?
			if ( texinfo > Context.TexInfo?.Count() )
			{
				texinfo = face.TexInfo;
			}

			var material = $"materials/dev/reflectivity_30.vmat";

			// still invalid, just use default material
			if ( texinfo > Context.TexInfo?.Count() )
			{
				//Log.Info( $"skipping face with invalid texinfo: {texinfo}" );
			}
			else
			{
				// get texture/material for face
				var texdata = Context.TexInfo?.ElementAtOrDefault( texinfo ).TexData;

				if ( texdata is null )
					continue;

				var stringtableindex = Context.TexDataStringTable?.ElementAtOrDefault( texdata.Value );

				if ( stringtableindex is null )
					continue;

				material = $"materials/{Context.TexDataStringData.FromStringTableIndex( stringtableindex.Value ).ToLower()}.vmat";
			}

			var verts = new List<Vector3>();

			for ( int i = 0; i < face.EdgeCount; i++ )
			{
				var edge = geo.SurfaceEdges.ElementAt( face.FirstEdge + i );

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
				var meshvert = new MeshVertex();
				meshvert.Position = vert + origin;
				polymesh.Vertices.Add( meshvert );
				indices.Add( polymesh.Vertices.Count() - 1 );
			}

			indices.Reverse();

			var meshface = new MeshFace( indices, Material.Load( material ) );
			polymesh.Faces.Add( meshface );
		}

		// no valid faces in mesh
		if ( !polymesh.Faces.Any() )
		{
			Log.Error( $"ConstructModel failed! No valid faces constructed!" );
			return null;
		}

		var mapmesh = new MapMesh( Hammer.ActiveMap );
		mapmesh.ConstructFromPolygons( polymesh );
		mapmesh.Name = name;

		return mapmesh;
	}
}
