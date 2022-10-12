namespace BspImport.Builder;

public partial class MapBuilder
{
	protected ImportContext Context { get; set; }

	public MapBuilder( ImportContext context )
	{
		Context = context;
	}

	/// <summary>
	/// Builds the final decompiled context inside of hammer. This will spawn world geometry and map entities, including parsed static props and brush entities.
	/// </summary>
	public void Build()
	{
		// prepares the polygon meshes
		BuildPolygonMeshes();

		// builds entities, including prop static and brush entities
		BuildEntities();

		// build map worldspawn geometry (model 0)
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

	/// <summary>
	/// Builds the map world geometry of the current context, using the previously cached PolygonMesh.
	/// </summary>
	protected virtual void BuildGeometry()
	{
		var polyMesh = Context.CachedPolygonMeshes?[0];

		if ( polyMesh is null )
		{
			Log.Error( $"Tried building WorldSpawn geometry, but Context has no Cached Worldspawn PolygonMesh!" );
			return;
		}

		var mapMesh = new MapMesh( Hammer.ActiveMap );
		mapMesh.ConstructFromPolygons( polyMesh );
		mapMesh.Name = $"worldspawn";
	}
}
