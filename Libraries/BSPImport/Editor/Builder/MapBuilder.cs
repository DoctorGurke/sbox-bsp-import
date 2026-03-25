namespace BspImport.Builder;

public partial class MapBuilder
{
	protected ImportContext Context { get; set; }

	public MapBuilder( ImportContext context )
	{
		Context = context;
	}

	/// <summary>
	/// Builds the final decompiled context insto the active editor scene. This will spawn world geometry and map entities, including parsed static props and brush entities.
	/// </summary>
	public void Build( GameObject? parent = null )
	{
		using var scope = SceneEditorSession.Scope();

		var name = Context.Name;

		var root = new GameObject( parent, true, name );

		// prepares bsp model meshes (brush entities)
		BuildModelMeshes();

		// build map worldspawn geometry (model 0)
		var worldspawn = BuildWorldGeometry();
		worldspawn.SetParent( root );

		// builds entities, including prop static and brush entities
		BuildEntities( worldspawn );

	}

	/// <summary>
	/// Builds the map world geometry of the current context, using the previously cached PolygonMesh.
	/// </summary>
	protected virtual GameObject BuildWorldGeometry()
	{
		//var mapMesh = new MapMesh( Map );
		var worldspawnMeshes = ConstructWorldspawn().ToList();

		Log.Info( $"MeshComponents: {worldspawnMeshes.Count}" );

		var worldspawn = new GameObject( true, "worldspawn" );

		foreach ( var mesh in worldspawnMeshes )
		{
			var meshComponent = worldspawn.Components.Create<MeshComponent>();
			meshComponent.Mesh = mesh;
		}

		//	if ( worldspawnMesh is null )
		//		return;

		//mapMesh.ConstructFromPolygons( worldspawnMesh );
		//	mapMesh.Name = $"worldspawn";

		return worldspawn;
	}
}
