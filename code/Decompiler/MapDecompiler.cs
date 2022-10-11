namespace BspImport.Decompiler;

[MapDecompiler( "Default" )]
public partial class MapDecompiler
{
	protected DecompilerContext Context { get; set; }

	public MapDecompiler( DecompilerContext context )
	{
		Context = context;
	}

	public virtual void Decompile( string file )
	{
		var data = File.ReadAllBytes( file );
		Context.Data = data;

		var parser = new ByteParser( data );

		// parse bsp header
		var ident = parser.Read<int>();
		var mapversion = parser.Read<int>();

		// 64 lump headers
		for ( int i = 0; i < 64; i++ )
		{
			if ( !Enum.IsDefined( typeof( LumpType ), i ) )
			{
				// skip lump header (offset, length, verion, fourCC)
				parser.Skip( sizeof( int ) * 3 + 4 );
				continue;
			}

			var offset = parser.Read<int>();
			var length = parser.Read<int>();
			var version = parser.Read<int>();
			parser.Skip( 4 ); // fourCC

			var lumpdata = data.Take( new Range( offset, offset + length ) );

			var parsed = ParseLump( i, lumpdata, version );

			if ( parsed is null )
				continue;

			Context.Lumps[i] = parsed;
		}

		var revision = parser.Read<int>();

		Log.Info( $"### DECOMPILED BSP: [ident: {ident} version: {mapversion} revision: {revision}]" );
	}
}
