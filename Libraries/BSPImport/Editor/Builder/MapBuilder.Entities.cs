namespace BspImport.Builder;

public partial class MapBuilder
{
	/// <summary>
	/// Build entities parsed from entity lump and static props
	/// </summary>
	protected virtual void BuildEntities( GameObject parent )
	{
		if ( Context.Entities is null )
			return;

		foreach ( var ent in Context.Entities )
		{
			if ( ent.ClassName is null )
				continue;

			// don't do shit with the worldspawn ent
			if ( ent.ClassName == "worldspawn" )
				continue;

			using var scope = SceneEditorSession.Scope();

			// props and brush entities
			if ( ent.Model is not null )
			{
				var isBrushEntity = ent.Model.StartsWith( '*' );
				var isStaticProp = ent.ClassName.Contains( "static" );

				if ( isBrushEntity && Context.Settings.ImportBrushEntities )
				{
					var brushEntity = new GameObject( true, ent.ClassName );
					brushEntity.SetParent( parent );
					brushEntity.WorldPosition = ent.Position;
					brushEntity.WorldRotation = ent.Angles.ToRotation();

					var propComponent = brushEntity.Components.Create<Prop>();
					var modelIndex = int.Parse( ent.Model.TrimStart( '*' ) );
					var polyMesh = Context.CachedPolygonMeshes?[modelIndex];

					if ( polyMesh is null )
						continue;

					propComponent.Model = polyMesh!.Rebuild();

					propComponent.IsStatic = true;
				}
				else if ( isStaticProp && Context.Settings.ImportStaticProps )
				{
					var staticProp = new GameObject( true, ent.ClassName );
					staticProp.SetParent( parent );
					staticProp.WorldPosition = ent.Position;
					staticProp.WorldRotation = ent.Angles.ToRotation();

					var propComponent = staticProp.Components.Create<Prop>();

					var model = Model.Load( ent.Model!.Replace( ".mdl", ".vmdl" ) );
					propComponent.Model = (model is null || model.IsError) ? Model.Error : model;
					propComponent.IsStatic = true;
				}

			}

			// regular entity
			//var mapent = new MapEntity( Map );
			//mapent.ClassName = ent.ClassName;
			//mapent.Position = ent.Position;
			//mapent.Angles = ent.Angles;
			//mapent.Name = ent.GetValue( "targetname" ) ?? "";

			//foreach ( var kvp in ent.Data )
			//{
			//	//mapent.SetKeyValue( kvp.Key, kvp.Value );
			//}
		}
	}
}
