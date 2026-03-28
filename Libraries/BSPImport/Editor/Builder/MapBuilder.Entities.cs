namespace BspImport.Builder;

public partial class MapBuilder
{
	private static HashSet<string> EntityClassBlacklist = new()
	{
				"info_node",
				"info_node_air",
				"env_sun",
				"sky_camera",
				"path_track",
				"water_lod_control",
				"func_areaportal",
				"shadow_control",
				"env_skypaint",
				"lua_run",
				"path_corner",
				"info_hint",
				"info_node_air_hint",
				"info_node_climb",
				"info_node_hint",
				"filter_multi",
				"point_template",
				"filter_activator_class",
				"point_message",
				"item_item_crate",
				"game_round_win",
				"filter_activator_tfteam",
				"item_ammopack_small",
				"item_ammopack_medium",
				"item_ammopack_full",
				"item_healthkit_small",
				"item_healthkit_medium",
				"item_healthkit_full",
				"info_player_teamspawn",
				"team_control_point",
				"point_devshot_camera",
				"info_observer_point",
				"info_intermission",
				"team_round_timer",
				"team_control_point_master",
				"item_teamflag",
				"info_null",
				"game_intro_viewpoint",
				"info_player_terrorist",
				"info_player_counterterrorist",
	};

	/// <summary>;
	/// Build entities parsed from entity lump and static props
	/// </summary>
	protected virtual void BuildEntities( GameObject parent )
	{
		if ( Context.Entities is null )
			return;

		var entityParent = new GameObject( parent, true, "entities" );
		var unhandledEntities = new HashSet<string>();

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



			// skip some useless entities
			if ( EntityClassBlacklist.Contains( ent.ClassName ) )
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
							var pitch = ent.GetValue( "pitch" )?.ToFloat() ?? ent.Angles.pitch;
							var forward = ent.Angles.WithPitch( pitch ).ToRotation().Forward;
							sunObj.WorldRotation = Rotation.LookAt( -forward );

							var light = sunObj.Components.Create<DirectionalLight>();

							// fetch color
							var lightString = ent.GetValue( "_light" );
							var colorVec = lightString is not null ? Vector4.Parse( ent.GetValue( "_light" ) ) : new Vector4( 1.0f );
							var color = Color.FromBytes( (int)colorVec.x, (int)colorVec.y, (int)colorVec.z );
							light.LightColor = color.WithAlpha( 1.0f );
						}
						break;

					default:
						if ( !unhandledEntities.Contains( ent.ClassName ) )
						{
							unhandledEntities.Add( ent.ClassName );
							Log.Warning( $"unhandled entity class: {ent.ClassName}" );
						}
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
