namespace BspImport.Decompiler;

[MapDecompiler( "Default" )]
public partial class MapDecompiler
{
	protected DecompilerContext Context { get; set; }

	public MapDecompiler( DecompilerContext context )
	{
		Context = context;
	}

	public virtual void Decompile()
	{
		var reader = new BinaryReader( new MemoryStream( Context.Data ) );

		// parse bsp header
		var ident = reader.ReadInt32();
		var mapversion = reader.ReadInt32();

		// 64 lump headers
		for ( int i = 0; i < 64; i++ )
		{
			var offset = reader.ReadInt32();
			var length = reader.ReadInt32();
			var version = reader.ReadInt32();
			reader.Skip( 4 ); // fourCC

			// don't bother
			if ( !Enum.IsDefined( typeof( LumpType ), i ) )
			{
				continue;
			}

			// prepare lump data
			byte[] lumpData = new byte[length];
			Array.Copy( Context.Data, offset, lumpData, 0, length );

			var parsed = ParseLump( i, lumpData, version );

			if ( parsed is null )
				continue;

			Context.Lumps[i] = parsed;
		}

		var revision = reader.ReadInt32();

		Log.Info( $"### DECOMPILED BSP: [ident: {ident} version: {mapversion} revision: {revision}]" );

		Context.Decompiled = true;
	}
}
