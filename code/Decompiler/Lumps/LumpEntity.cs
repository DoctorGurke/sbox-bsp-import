namespace BspImport.Decompiler.Lumps;

public class LumpEntity
{
	public List<KeyValuePair<string, string>> Data { get; set; } = new List<KeyValuePair<string, string>>();

	public string? GetValue( string key )
	{
		if ( !Data.Any() )
		{
			return null;
		}

		var find = Data.Where( x => x.Key == key );

		if ( !find.Any() )
		{
			return null;
		}

		var val = find.FirstOrDefault().Value;
		return val;
	}

	private void TryReplaceData( string key, string value )
	{
		var index = Data.FindIndex( x => x.Key == key );

		if ( index <= -1 )
		{
			Data.Add( new KeyValuePair<string, string>( key, value ) );
		}
		else
		{
			Data[index] = new KeyValuePair<string, string>( key, value );
		}
	}

	public void SetClassName( string name ) => TryReplaceData( "classname", name );

	public void SetPosition( Vector3 origin ) => TryReplaceData( "origin", $"[{origin.x} {origin.y} {origin.z}]" );

	public void SetAngles( Angles angles ) => TryReplaceData( "angles", $"{angles.pitch},{angles.yaw},{angles.roll}" );

	public void SetModel( string model ) => TryReplaceData( "model", model );

	public string? ClassName => GetValue( "classname" );
	public Vector3 Position => Vector3.Parse( $"[{GetValue( "origin" ) ?? ""}]" );

	private Angles ConstructAngles()
	{
		var split = GetValue( "angles" )?.Split( ' ' );

		if ( split is null || split.Length != 3 )
			return Angles.Zero;

		return Angles.Parse( $"{split[0]},{split[1]},{split[2]}" );
	}

	public Angles Angles => ConstructAngles();
}
