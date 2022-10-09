namespace BspImport.Decompiler.Lumps;

public class EntityLump : BaseLump
{
	public EntityLump( DecompilerContext context, IEnumerable<byte> data, int version = 0 ) : base( context, data, version )
	{
		var pairs = Encoding.ASCII.GetString( data.ToArray() );
		var ents = FromKeyValues( pairs );

		Context.Entities = ents;
	}

	private static IEnumerable<LumpEntity> FromKeyValues( string keyvalues )
	{
		var data = keyvalues.Split( '\n' );

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
						ent.Data.Add( new( kv[0].Trim( '\"' ), kv[1].Trim( '\"' ) ) );
					}
					break;
			}
		}
	}
}
