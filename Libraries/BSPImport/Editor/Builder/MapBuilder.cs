namespace BspImport.Builder;

public partial class MapBuilder
{
	protected ImportContext Context { get; set; }

	public MapBuilder( ImportContext context )
	{
		Context = context;

		SetupEntityHandlers();
	}

	/// <summary>
	/// Builds the final decompiled context insto the active editor scene. This will spawn world geometry and map entities, including parsed static props and brush entities.
	/// </summary>
	public void Build( GameObject? parent = null )
	{
		using var scope = SceneEditorSession.Scope();

		var name = Context.Name;

		var root = new GameObject( parent, true, name );


		// build map worldspawn geometry (model 0), including displacements
		if ( Context.Settings.ImportWorldGeometry )
		{

			BuildWorldGeometry( root );
		}

		// builds entities, including prop static and brush entities
		if ( Context.Settings.ImportEntities )
		{
			var entities = new GameObject( root, true, "Entities" );

			// prepares bsp model meshes (brush entities)
			BuildModelMeshes();
			BuildEntities( entities );
		}
	}


}
