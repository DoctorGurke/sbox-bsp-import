namespace BspImport.Builder;

public partial class MapBuilder
{
	protected ImportContext Context { get; set; }
	protected MapDocument Map { get; set; }

	public MapBuilder( ImportContext context, MapDocument map )
	{
		Context = context;
		Map = map;
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
	/// Builds the map world geometry of the current context, using the previously cached PolygonMesh.
	/// </summary>
	protected virtual void BuildGeometry()
	{
		var mapMesh = new MapMesh( Map );
		var worldspawnMesh = ConstructWorldspawn();

		if ( worldspawnMesh is null )
			return;

		mapMesh.ConstructFromPolygons( worldspawnMesh );
		mapMesh.Name = $"worldspawn";
	}
}
