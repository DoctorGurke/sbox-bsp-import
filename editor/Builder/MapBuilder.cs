using Editor;

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

		// build map worldspawn geometry (model 0)
		var worldspawn = BuildGeometry();

		// builds entities, including prop static and brush entities
		BuildEntities( worldspawn );

	}

	/// <summary>
	/// Builds the map world geometry of the current context, using the previously cached PolygonMesh.
	/// </summary>
	protected virtual GameObject BuildGeometry()
	{
		//var mapMesh = new MapMesh( Map );
		var worldspawnMeshes = ConstructWorldspawn();

		Log.Info( $"MeshComponents: {worldspawnMeshes.Count()}" );

		using var scope = SceneEditorSession.Scope();
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
