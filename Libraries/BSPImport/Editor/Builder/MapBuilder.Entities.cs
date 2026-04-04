namespace BspImport.Builder;

using BspImport.Builder.Entities;
using System.Threading;
using System.Threading.Tasks;

public partial class MapBuilder
{
	/// <summary>
	/// Register common entity class handlers
	/// </summary>
	private void SetupEntityHandlers()
	{
		Handlers.Clear();

		Handlers.Add( "prop_static", BaseEntities.HandleStaticPropEntity );
		Handlers.Add( "prop_physics", BaseEntities.HandlePhysicsPropEntity );
		Handlers.Add( "prop_dynamic", BaseEntities.HandleDynamicPropEntity );
		Handlers.Add( "info_player_start", BaseEntities.HandlePlayerStartEntity );
		Handlers.Add( "light", BaseEntities.HandleLightEntity );
		Handlers.Add( "light_spot", BaseEntities.HandleSpotLightEntity );
		Handlers.Add( "light_environment", BaseEntities.HandleLightEnvironmentEntity );
	}

	private readonly Dictionary<string, Action<GameObject, LumpEntity, GameObject, ImportSettings>> Handlers = new();

	// TODO: add filter system including target game so these rules can be split up
	private List<string> EntityClassBlacklist = new()
	{
				"worldspawn",
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
				"func_areaportalwindow"
	};

	private bool IsAllowedEntity( LumpEntity ent )
	{
		if ( ent.ClassName is null || EntityClassBlacklist.Contains( ent.ClassName ) )
			return false;

		if ( ent.ClassName.Contains( "logic" ) )
			return false;

		var isModel = BaseEntities.IsModelEntity( ent );

		var leafIndex = TreeParse.FindLeafIndex( ent.Position );
		var leafArea = Context.Leafs![leafIndex].Area;

		var cullSkyboxModel = isModel && Context.Settings.CullSkybox ? Context.SkyboxAreas.Contains( leafArea ) : false;
		var cullModel = isModel ? !Context.Settings.LoadModels : false;
		return !cullModel && !cullSkyboxModel;
	}

	/// <summary>;
	/// Build entities parsed from entity lump and static props. Does not include brush entities.
	/// </summary>
	protected virtual async Task BuildEntities( GameObject _parent, IProgressSection progress, int entitiesPerFrame, CancellationToken token )
	{
		if ( Context.Entities is null )
			return;

		// for deduplicating "unhandled entity" messages
		var unhandledEntities = new HashSet<string>();

		if ( token.IsCancellationRequested )
			return;

		Log.Info( "Building Entities..." );
		progress.Title = "Building Entities...";

		var brushEntities = Context.Entities.Where( ent => ent.IsBrushEntity && IsAllowedEntity( ent ) );
		var pointEntities = Context.Entities.Where( ent => !ent.IsBrushEntity && IsAllowedEntity( ent ) );

		int total = brushEntities.Count() + pointEntities.Count();
		int count = 0;

		progress.TotalCount = total;
		progress.Current = count;

		if ( brushEntities.Any() )
		{
			var parent = new GameObject( _parent, true, "Mesh Entities" );

			progress.Subtitle = $"Building {brushEntities.Count()} Mesh Entities...";

			foreach ( var ent in brushEntities )
			{
				if ( token.IsCancellationRequested )
					return;

				if ( ent.ClassName is null )
					continue;

				// ... so we can gurantee they get their meshes
				var meshObj = CreateBrushEntity( ent, parent );

				count++;
				progress.Current = count;

				if ( !meshObj.IsValid() )
					continue;

				// try to find handlers based on classname
				if ( Handlers.TryGetValue( ent.ClassName, out var handler ) )
				{
					// apply class components via registered handler
					handler.Invoke( meshObj, ent, parent, Context.Settings );
				}
				else
				{
					if ( !unhandledEntities.Contains( ent.ClassName ) )
					{
						unhandledEntities.Add( ent.ClassName );
						Log.Warning( $"unhandled entity class: {ent.ClassName}" );
					}
				}

				if ( count % entitiesPerFrame == 0 )
				{
					await GameTask.Yield();
				}
			}
		}

		if ( token.IsCancellationRequested )
			return;

		if ( pointEntities.Any() )
		{
			var parent = new GameObject( _parent, true, "Point Entities" );

			progress.Subtitle = $"Building {pointEntities.Count()} Point Entities...";

			foreach ( var ent in pointEntities )
			{
				if ( token.IsCancellationRequested )
					return;

				if ( ent.ClassName is null )
					continue;

				// try to find handlers based on classname
				if ( Handlers.TryGetValue( ent.ClassName, out var handler ) )
				{
					var newObj = CreatePointEntity( ent, parent );

					// apply class components via registered handler
					handler.Invoke( newObj, ent, parent, Context.Settings );
				}
				else
				{
					if ( !unhandledEntities.Contains( ent.ClassName ) )
					{
						unhandledEntities.Add( ent.ClassName );
						Log.Warning( $"unhandled entity class: {ent.ClassName}" );
					}
				}

				// unhandled entities still count towards total
				count++;
				progress.Current = count;


				if ( count % entitiesPerFrame == 0 )
				{
					await GameTask.Yield();
				}
			}
		}
	}

	/// <summary>
	/// Create a basic point entity with Position and Rotation.
	/// </summary>
	/// <param name="ent"></param>
	/// <param name="parent"></param>
	/// <returns></returns>
	private static GameObject CreatePointEntity( LumpEntity ent, GameObject parent )
	{
		var obj = new GameObject( parent, true, ent.TargetName );
		obj.WorldPosition = ent.Position;
		obj.WorldRotation = ent.Angles.ToRotation();

		return obj;
	}

	/// <summary>
	/// Creates a GameObject based on a brush entity. If the entity doesn't have a valid model this will return null.
	/// </summary>
	/// <param name="ent"></param>
	/// <param name="parent"></param>
	/// <returns></returns>
	private GameObject? CreateBrushEntity( LumpEntity ent, GameObject parent )
	{
		if ( ent.Model is null || !Context.Settings.ImportEntities )
			return null;

		var modelIndex = int.Parse( ent.Model.TrimStart( '*' ) );
		var polyMesh = Context.CachedPolygonMeshes?[modelIndex];

		if ( polyMesh is null )
			return null;

		var brushEntity = CreatePointEntity( ent, parent );

		var meshComponent = brushEntity.Components.Create<MeshComponent>();
		meshComponent.Mesh = polyMesh;

		CenterMeshOrigin( meshComponent );

		//var propComponent = brushEntity.Components.Create<Prop>();
		//propComponent.Model = polyMesh.Rebuild();
		//propComponent.IsStatic = true;

		return brushEntity;
	}

}
