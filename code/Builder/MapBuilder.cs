using BspImport.Decompiler;
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
				Log.Info( $"brush entity {index}" );
				ConstructModel( index, ent.Position, ent.Model );
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

	private MapMesh ConstructModel( int index, string? name = null ) => ConstructModel( index, Vector3.Zero, name );

	private MapMesh ConstructModel( int index, Vector3 origin, string? name = null )
	{
		var geo = Context.MapGeometry;

		if ( Context.Models is null || geo.VertexPositions is null || geo.SurfaceEdges is null || geo.EdgeIndices is null || geo.Faces is null || geo.OriginalFaces is null )
		{
			throw new Exception( "No valid map geometry to construct!" );
		}

		if ( index > Context.Models.Count() )
		{
			throw new Exception( $"Tried to construct map model with index: {index}. Exceeds available Models!" );
		}

		var model = Context.Models.ElementAt( index );

		var polymesh = new PolygonMesh();

		// get original faces from split faces
		var originalfaces = new HashSet<int>();
		for ( int i = 0; i < model.FaceCount; i++ )
		{
			originalfaces.Add( geo.Faces.ElementAt( model.FirstFace + i ).OriginalFaceIndex );
		}

		// construct original faces
		foreach ( var origface in originalfaces )
		{
			var face = geo.OriginalFaces.ElementAt( origface );

			// only construct valid primitives
			if ( face.EdgeCount < 3 )
				continue;

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
			var meshverts = new List<MeshVertex>();
			foreach ( var vert in verts )
			{
				var meshvert = new MeshVertex();
				meshvert.Position = vert + origin;
				meshverts.Add( meshvert );
			}

			meshverts.Reverse();
			polymesh.AddFace( meshverts.ToArray() );
		}

		var mapmesh = new MapMesh( Hammer.ActiveMap );
		mapmesh.ConstructFromPolygons( polymesh );
		mapmesh.Name = name;

		return mapmesh;
	}
}
