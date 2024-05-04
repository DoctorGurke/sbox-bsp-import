using Editor;

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

			// props and brush entities
			if ( ent.Model is not null )
			{
				using var scope = SceneEditorSession.Scope();
				//TODO: parent to worldspawn game object
				var prop = new GameObject( true, ent.ClassName );
				prop.SetParent( parent );
				prop.Transform.Position = ent.Position;
				prop.Transform.Rotation = ent.Angles.ToRotation();

				var propComponent = prop.Components.Create<Prop>();

				if ( ent.Model.StartsWith( '*' ) )
				{
					var modelIndex = int.Parse( ent.Model.TrimStart( '*' ) );
					var polyMesh = Context.CachedPolygonMeshes?[modelIndex];

					if ( polyMesh is null )
						continue;

					propComponent.Model = polyMesh!.Rebuild();

					propComponent.IsStatic = true;
				}
				else
				{
					var model = Model.Load( ent.Model!.Replace( ".mdl", ".vmdl" ) );
					propComponent.Model = (model is null || model.IsError) ? Model.Error : model;
				}

				// prop_static
				propComponent.IsStatic = ent.ClassName.Contains( "static" );
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
