namespace BspImport.Decompiler;

[MapDecompiler( "Default" )]
public static partial class MapDecompiler
{
	private static int VBSP = ('P' << 24) + ('S' << 16) + ('B' << 8) + 'V';

	public static void Decompile( string file )
	{
		var data = File.ReadAllBytes( file );
		DecompilerContext.Data = data;

		var parser = new ByteParser( data );

		// parse bsp header
		var ident = parser.Read<int>();
		var mapversion = parser.Read<int>();

		// 64 lump headers
		for ( int i = 0; i < 64; i++ )
		{
			var offset = parser.Read<int>();
			var length = parser.Read<int>();
			var version = parser.Read<int>();
			parser.Skip( 4 ); // fourCC

			var lumpdata = data.Take( new Range( offset, offset + length ) );

			var parsed = ParseLump( i, lumpdata, version );

			if ( parsed is null )
				continue;

			DecompilerContext.Lumps[i] = parsed;
		}

		var revision = parser.Read<int>();

		Log.Info( $"ident: {ident} version: {mapversion} revision: {revision}" );
	}
}
