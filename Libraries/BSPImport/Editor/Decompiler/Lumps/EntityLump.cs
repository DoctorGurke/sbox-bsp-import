namespace BspImport.Decompiler.Lumps;

public class EntityLump : BaseLump
{
	public EntityLump( ImportContext context, byte[] data, int version = 0 ) : base( context, data, version ) { }

	protected override void Parse( BinaryReader reader )
	{
		var pairs = Encoding.ASCII.GetString( Data );
		var ents = FromKeyValues( pairs );

		Log.Info( $"ENTITIES: {ents.Count()}" );

		Context.Entities = ents.ToArray();
	}

	private static IEnumerable<LumpEntity> FromKeyValues( string keyValues )
	{
		var data = keyValues.Split( '\n' );

		LumpEntity? ent = null;

		foreach ( var line in data )
		{
			switch ( line )
			{
				case "{":
					ent = new LumpEntity();
					break;
				case "}":
					if ( ent is not null )
						yield return ent;
					break;
				default:
					if ( ent is null )
						continue;

					var kv = line.Split( ' ', 2 );
					if ( kv.Length == 2 )
					{
						ent?.Data.Add( new( kv[0].Trim( '\"' ), kv[1].Trim( '\"' ) ) );
					}
					break;
			}
		}
	}
}

public class LumpEntity
{
	public List<KeyValuePair<string, string>> Data { get; private set; } = new();

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

	// helpers for constructing artificial entities, like prop static
	public void SetClassName( string name ) => TryReplaceData( "classname", name );

	public void SetPosition( Vector3 origin ) => TryReplaceData( "origin", $"[{origin.x} {origin.y} {origin.z}]" );

	public void SetAngles( Angles angles ) => TryReplaceData( "angles", $"{angles.pitch},{angles.yaw},{angles.roll}" );

	public void SetModel( string model ) => TryReplaceData( "model", model );

	// getting data for making actual MapEntity refs, parsed from kv data
	public string? ClassName => GetValue( "classname" );
	public Vector3 Position => Vector3.Parse( $"[{GetValue( "origin" ) ?? ""}]" );

	private Angles ConstructAngles()
	{
		var split = GetValue( "angles" )?.Split( ' ' );

		if ( split is null || split.Length != 3 )
			return Angles.Zero;

		var angle = Angles.Parse( $"{split[0]},{split[1]},{split[2]}" );

		return angle;
	}

	public Angles Angles => ConstructAngles();

	public string? Model => GetValue( "model" );
}
