namespace BspImport.Decompiler.Lumps;

public class GameLumpHeader : BaseLump
{
	public GameLumpHeader( DecompilerContext context, IEnumerable<byte> data, int version = 0 ) : base( context, data, version ) { }

	protected override void Parse( ByteParser data )
	{
		var count = data.Read<int>();

		var list = new List<GameLump>();

		for ( int i = 0; i < count; i++ )
		{
			var lump = data.ReadBytes( 16 );
			var gameLump = new GameLump( Context, lump );
			list.Add( gameLump );
		}

		Context.GameLumps = list;
	}
}
