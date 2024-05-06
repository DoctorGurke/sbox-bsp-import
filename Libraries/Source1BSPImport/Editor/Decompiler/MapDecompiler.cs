namespace BspImport.Decompiler;

[MapDecompiler( "Default" )]
public partial class MapDecompiler
{
	protected ImportContext Context { get; set; }

	public MapDecompiler( ImportContext context )
	{
		Context = context;
	}

	/// <summary>
	/// Begin decompiling the bsp file structure. Reads bsp header and 64 sequential lump headers. Will jump to read section of bsp as lump type along the way if we know about the lump type.
	/// </summary>
	public virtual void Decompile()
	{
		Log.Info( $"Decompiling Context..." );

		var reader = new BinaryReader( new MemoryStream( Context.Data ) );

		// read bsp header
		var ident = reader.ReadInt32();
		var mapversion = reader.ReadInt32();

		// iterate all 64 possible lump headers
		for ( int i = 0; i < 64; i++ )
		{
			// read i lump header to get info about lump in bsp
			var offset = reader.ReadInt32(); // offset into bsp
			var length = reader.ReadInt32(); // length in bytes
			var version = reader.ReadInt32(); // lump versions can affect data structure
			reader.Skip( 4 ); // fourCC - unused

			// only attempt to decompile lumps we know about
			if ( !Enum.IsDefined( typeof( LumpType ), i ) )
			{
				continue;
			}

			// prepare lump data section
			byte[] lumpData = new byte[length];
			Array.Copy( Context.Data, offset, lumpData, 0, length );

			BaseLump? lump = null;
			try
			{
				ParseLump( i, lumpData, version );
			}
			catch ( Exception ex )
			{
				Log.Error( $"Failed decompiling lump: {(LumpType)i} {ex}" );
			}

			if ( lump is null )
				continue;

			// store in context after we're done with all lumps
			Context.Lumps[i] = lump;
		}

		var revision = reader.ReadInt32();

		Log.Info( $"Finished Decompiling: [ident: {ident} version: {mapversion} revision: {revision}]" );
	}
}
