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

		var entityParent = new GameObject( parent, true, "entities" );

		foreach ( var ent in Context.Entities )
		{
			if ( ent.ClassName is null )
				continue;

			// don't do shit with the worldspawn ent
			if ( ent.ClassName == "worldspawn" )
				continue;

			// skip logic entities
			if ( ent.ClassName.Contains( "logic" ) )
				continue;

			var blacklist = new List<string>()
			{
				"info_node",
				"info_node_air",
				"env_sun"
			};

			// skip some useless entities
			if ( blacklist.Contains( ent.ClassName ) )
				continue;

			// props and brush entities
			if ( ent.Model is not null )
			{
				HandleModelEntity( ent, entityParent );
			}
			else
			{
				var targetname = ent.GetValue( "targetname" ) ?? ent.ClassName;

				switch ( ent.ClassName )
				{
					case "info_player_start":
						{
							var playerStart = new GameObject( entityParent, true, targetname );
							playerStart.WorldPosition = ent.Position;
							playerStart.WorldRotation = ent.Angles.ToRotation();
							playerStart.Components.Create<SpawnPoint>();
						}
						break;

					case "light":
						{
							var lightObj = new GameObject( entityParent, true, targetname );
							lightObj.WorldPosition = ent.Position;

							var light = lightObj.Components.Create<PointLight>();

							// fetch qattenuation and distance
							light.Attenuation = ent.GetValue( "_quadratic_attn" )?.ToFloat() ?? 1f;
							var distance = ent.GetValue( "_distance" )?.ToInt();
							if ( distance is not null && distance != 0 )
								light.Radius = distance.Value;

							// fetch color
							var lightString = ent.GetValue( "_light" );
							var colorVec = lightString is not null ? Vector4.Parse( ent.GetValue( "_light" ) ) : new Vector4( 1.0f );
							var color = Color.FromBytes( (int)colorVec.x, (int)colorVec.y, (int)colorVec.z );
							light.LightColor = color.WithAlpha( 1.0f );

						}
						break;

					case "light_environment":
						{
							var sunObj = new GameObject( entityParent, true, targetname );
							sunObj.WorldPosition = ent.Position;
							sunObj.WorldRotation = ent.Angles.WithPitch( 1 - ent.Angles.pitch ).ToRotation();

							var light = sunObj.Components.Create<DirectionalLight>();

							// fetch color
							var lightString = ent.GetValue( "_light" );
							var colorVec = lightString is not null ? Vector4.Parse( ent.GetValue( "_light" ) ) : new Vector4( 1.0f );
							var color = Color.FromBytes( (int)colorVec.x, (int)colorVec.y, (int)colorVec.z );
							light.LightColor = color.WithAlpha( 1.0f );
						}
						break;

					default:
						Log.Warning( $"unhandled entity: {ent.ClassName} {(targetname != ent.ClassName ? targetname : null)}" );
						break;
				}
			}
		}
	}

	private void HandleModelEntity( LumpEntity ent, GameObject parent )
	{
		if ( ent.ClassName is null || ent.Model is null )
			return;

		if ( ent.Model.StartsWith( '*' ) )
		{
			var brushEntity = new GameObject( true, ent.ClassName );
			brushEntity.SetParent( parent );
			brushEntity.WorldPosition = ent.Position;
			brushEntity.WorldRotation = ent.Angles.ToRotation();

			var propComponent = brushEntity.Components.Create<Prop>();
			var modelIndex = int.Parse( ent.Model.TrimStart( '*' ) );
			var polyMesh = Context.CachedPolygonMeshes?[modelIndex];

			if ( polyMesh is null )
				return;

			propComponent.Model = polyMesh!.Rebuild();

			propComponent.IsStatic = true;
		}
		else
		{
			if ( !Context.Settings.LoadModels )
				return;

			var staticProp = new GameObject( true, ent.ClassName );
			staticProp.SetParent( parent );
			staticProp.WorldPosition = ent.Position;
			staticProp.WorldRotation = ent.Angles.ToRotation();

			var propComponent = staticProp.Components.Create<Prop>();

			var model = Model.Load( ent.Model!.Replace( ".mdl", ".vmdl" ) );
			propComponent.Model = (model is null || model.IsError) ? Model.Error : model;

			propComponent.IsStatic = ent.ClassName.Contains( "static" );
		}
	}
}
