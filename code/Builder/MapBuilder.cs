namespace BspImport.Builder;

public partial class MapBuilder
{
	protected DecompilerContext Context { get; set; }

	public MapBuilder( DecompilerContext context )
	{
		Context = context;
	}

	/// <summary>
	/// Caches all materials inside the Context's TexDataStringData for later use.
	/// </summary>
	/// <remarks>Necessary to be able to generate PolygonMeshes in parallel, since Materials can only be loaded on the main thread.</remarks>
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

	/// <summary>
	/// Builds the final decompiled context inside of hammer. This will spawn world geometry and map entities, including parsed static props and brush entities.
	/// </summary>
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

			// skip point entities for now
			return;

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
		ThreadSafe.AssertIsMainThread( $"MapBuilder.Build()" );

		var polyMesh = Context.CachedPolygonMeshes?[0];

		if ( polyMesh is null )
		{
			Log.Error( $"Tried building WorldSpawn geometry, but Context has no Cached PolygonMeshes!" );
			return;
		}

		var mapMesh = new MapMesh( Hammer.ActiveMap );
		mapMesh.ConstructFromPolygons( polyMesh );
		mapMesh.Name = $"worldspawn";
	}
}
