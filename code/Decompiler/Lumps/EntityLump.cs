using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Tools.MapDoc;

namespace BspImport.Decompiler.Lumps;

public class EntityLump : BaseLump
{
	public EntityLump( IEnumerable<byte> data, int version = 0 ) : base( data, version )
	{
		var pairs = Encoding.ASCII.GetString( data.ToArray() );
		var ents = FromKeyValues( pairs );

		DecompilerContext.Entities = ents;
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
