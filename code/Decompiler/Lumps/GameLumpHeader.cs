namespace BspImport.Decompiler.Lumps;

public class GameLumpHeader : BaseLump
{
	public GameLumpHeader( DecompilerContext context, byte[] data, int version = 0 ) : base( context, data, version ) { }

	protected override void Parse( ByteParser data )
	{
		var count = data.Read<int>();

		var gameLumps = new GameLump[count];

		var list = new List<GameLump>();

		for ( int i = 0; i < count; i++ )
		{
			var lump = data.ReadBytes( 16 );
			var gameLump = new GameLump( Context, lump );
			gameLumps[i] = gameLump;
		}

		Context.GameLumps = gameLumps;
	}
}
