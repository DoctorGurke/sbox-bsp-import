namespace BspImport.Decompiler.Lumps;

public class GameLumpHeader : BaseLump
{
	public GameLumpHeader( ImportContext context, byte[] data, int version = 0 ) : base( context, data, version ) { }

	protected override void Parse( BinaryReader reader )
	{
		var count = reader.ReadInt32();

		var gameLumps = new GameLump[count];

		for ( int i = 0; i < count; i++ )
		{
			// each gamelump is 16 bytes
			var lump = reader.ReadBytes( 16 );
			var gameLump = new GameLump( Context, lump );
			gameLumps[i] = gameLump;
		}

		Context.GameLumps = gameLumps;
	}
}
