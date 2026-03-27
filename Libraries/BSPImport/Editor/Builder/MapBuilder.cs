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
		var worldspawn = new GameObject( true, "worldspawn" );

		var worldspawnMeshes = ConstructWorldspawn().ToList();
		var displacementMeshes = ConstructDisplacementMeshes().ToList();

		if ( displacementMeshes.Any() && Context.Settings.ImportDisplacements )
		{
			Log.Info( $"Displacement Meshes: {displacementMeshes.Count}" );
			var displacementParent = new GameObject( worldspawn, true, "displacements" );

			foreach ( var displacement in displacementMeshes )
			{
				var dispObject = new GameObject( displacementParent, true, "displacement" );
				var meshComponent = dispObject.Components.Create<MeshComponent>();
				meshComponent.Mesh = displacement;
				CenterMeshOrigin( meshComponent );
			}
		}

		if ( worldspawnMeshes.Any() )
		{
			Log.Info( $"World Meshes: {worldspawnMeshes.Count}" );
			var meshParent = new GameObject( worldspawn, true, "world_meshes" );
			foreach ( var mesh in worldspawnMeshes )
			{
				var meshObject = new GameObject( meshParent, true, "world_mesh" );
				var meshComponent = meshObject.Components.Create<MeshComponent>();
				meshComponent.Mesh = mesh;
				CenterMeshOrigin( meshComponent );
			}
		}

		return worldspawn;
	}
}
