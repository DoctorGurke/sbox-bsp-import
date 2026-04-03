namespace BspImport.Decompiler.Lumps;

public class GameLumpHeader : BaseLump
{
	public GameLumpHeader( ImportContext context, byte[] data, int version = 0 ) : base( context, data, version ) { }

	protected override void Parse( BinaryReader reader )
	{
		if ( reader.GetLength() < sizeof( int ) )
			return;

		var count = reader.ReadInt32();
		if ( count <= 0 )
			return;

		var gameLumps = new GameLump[count];

		for ( int i = 0; i < count; i++ )
		{
			if ( reader.GetLength() < 16 )
			{
				Log.Warning( $"[BSP] Game lump header truncated after {i} entries." );
				break;
			}

			// each gamelump is 16 bytes
			var lump = reader.ReadBytes( 16 );
			var gameLump = new GameLump( Context, lump );
			gameLumps[i] = gameLump;
		}

		Context.GameLumps = gameLumps;
	}
}
