namespace BspImport.Builder.Entities;

internal static class BaseEntities
{
	public static bool IsModelEntity( LumpEntity ent )
	{
		if ( ent.Model is null || ent.Model == string.Empty )
			return false;

		if ( !ent.ClassName!.Contains( "prop" ) )
			return false;

		return true;
	}

	/// <summary>
	/// prop_static
	/// </summary>
	public static void HandleStaticPropEntity( GameObject obj, LumpEntity ent, GameObject parent, ImportSettings settings )
	{
		var propComponent = obj.Components.Create<Prop>();

		var model = Model.Load( ent.Model!.Replace( ".mdl", ".vmdl" ) );
		propComponent.Model = model;
		propComponent.IsStatic = true;
	}

	/// <summary>
	/// prop_physics
	/// </summary>
	public static void HandlePhysicsPropEntity( GameObject obj, LumpEntity ent, GameObject parent, ImportSettings settings )
	{
		var propComponent = obj.Components.Create<Prop>();
		var model = Model.Load( ent.Model!.Replace( ".mdl", ".vmdl" ) );
		propComponent.Model = model;
		propComponent.IsStatic = false;

		// apply tint
		var tintVec = Vector3Int.Parse( ent.GetValue( "rendercolor" ) ?? "255 255 255" );
		var tintCol = Color.FromBytes( tintVec.x, tintVec.y, tintVec.z );
		propComponent.Tint = tintCol;

		// apply model scale
		var scale = ent.GetValue( "modelscale" )?.ToFloat() ?? 1.0f;
		propComponent.GameObject.WorldScale = new Vector3( scale );
	}

	/// <summary>
	/// prop_dynamic
	/// </summary>
	public static void HandleDynamicPropEntity( GameObject obj, LumpEntity ent, GameObject parent, ImportSettings settings )
	{
		var propComponent = obj.Components.Create<Prop>();
		var model = Model.Load( ent.Model!.Replace( ".mdl", ".vmdl" ) );
		propComponent.Model = model;
		propComponent.IsStatic = false;

		// apply tint
		var tintVec = Vector3Int.Parse( ent.GetValue( "rendercolor" ) ?? "255 255 255" );
		var tintCol = Color.FromBytes( tintVec.x, tintVec.y, tintVec.z );
		propComponent.Tint = tintCol;

		// apply model scale
		var scale = ent.GetValue( "modelscale" )?.ToFloat() ?? 1.0f;
		propComponent.GameObject.WorldScale = new Vector3( scale );

		propComponent.IsStatic = true;
	}

	/// <summary>
	/// info_player_start
	/// </summary>
	public static void HandlePlayerStartEntity( GameObject obj, LumpEntity ent, GameObject parent, ImportSettings settings )
	{
		obj.Components.Create<SpawnPoint>();
	}

	/// <summary>
	/// light
	/// </summary>
	public static void HandleLightEntity( GameObject obj, LumpEntity ent, GameObject parent, ImportSettings settings )
	{
		var light = obj.Components.Create<PointLight>();

		// fetch qattenuation and distance
		light.Attenuation = ent.GetValue( "_quadratic_attn" )?.ToFloat() ?? 1f;
		var distance = ent.GetValue( "_distance" )?.ToInt();
		if ( distance is not null && distance != 0 )
			light.Radius = distance.Value;

		// fetch color
		var lightString = ent.GetValue( "_light" );
		if ( lightString is not null )
		{
			Vector3Int colorVec = new( 255 );

			var components = lightString.Split( ' ' );
			if ( components.Length < 3 )
				return;

			colorVec = Vector3Int.Parse( $"{components[0]} {components[1]} {components[2]}" );

			var color = Color.FromBytes( colorVec.x, colorVec.y, colorVec.z );
			light.LightColor = color.WithAlpha( 1.0f );
		}

		if ( light.Attenuation == 0 )
		{
			light.Attenuation = 1;
		}
	}

	public static void HandleSpotLightEntity( GameObject obj, LumpEntity ent, GameObject parent, ImportSettings settings )
	{
		// apply -pitch light rotation
		obj.WorldRotation = ent.GetLightRotation();

		var light = obj.Components.Create<SpotLight>();

		// fetch qattenuation and distance
		light.Attenuation = ent.GetValue( "_quadratic_attn" )?.ToFloat() ?? 1f;
		var distance = ent.GetValue( "_distance" )?.ToInt();
		if ( distance is not null && distance != 0 )
			light.Radius = distance.Value;

		// fetch color
		var lightString = ent.GetValue( "_light" );
		if ( lightString is not null )
		{
			Vector3Int colorVec = new( 255 );

			var components = lightString.Split( ' ' );
			if ( components.Length < 3 )
				return;

			colorVec = Vector3Int.Parse( $"{components[0]} {components[1]} {components[2]}" );

			var color = Color.FromBytes( colorVec.x, colorVec.y, colorVec.z );
			light.LightColor = color.WithAlpha( 1.0f );
		}
	}

	/// <summary>
	/// light_environment
	/// </summary>
	public static void HandleLightEnvironmentEntity( GameObject obj, LumpEntity ent, GameObject parent, ImportSettings settings )
	{
		// apply -pitch light rotation
		obj.WorldRotation = ent.GetLightRotation();

		var light = obj.Components.Create<DirectionalLight>();

		// fetch color
		var lightString = ent.GetValue( "_light" );
		if ( lightString is not null )
		{
			Vector3Int colorVec = new( 255 );

			var components = lightString.Split( ' ' );
			if ( components.Length < 3 )
				return;

			colorVec = Vector3Int.Parse( $"{components[0]} {components[1]} {components[2]}" );

			var color = Color.FromBytes( colorVec.x, colorVec.y, colorVec.z );
			light.LightColor = color.WithAlpha( 1.0f );
		}
	}
}
