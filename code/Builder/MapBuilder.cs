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

				// create entity
				var brushEntity = new MapEntity( Hammer.ActiveMap );
				brushEntity.ClassName = ent.ClassName;
				brushEntity.Position = ent.Position;
				brushEntity.Angles = ent.Angles;

				// create and attach mesh, (tie mesh to entity)
				var mapMesh = new MapMesh( Hammer.ActiveMap );
				mapMesh.ConstructFromPolygons( polyMesh );
				mapMesh.Parent = brushEntity;
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
		var mapMesh = new MapMesh( Hammer.ActiveMap );
		var worldspawnMesh = ConstructWorldspawn();
		mapMesh.ConstructFromPolygons( worldspawnMesh );
		mapMesh.Name = $"worldspawn";
	}
}
