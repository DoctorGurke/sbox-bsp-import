namespace BspImport.Decompiler;

[MapDecompiler( "Default" )]
public partial class MapDecompiler
{
	protected ImportContext Context { get; set; }

	public MapDecompiler( ImportContext context )
	{
		Context = context;
	}

	public virtual void Decompile()
	{
		Log.Info( $"Decompiling Context..." );

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

			BaseLump? lump = null;
			try
			{
				ParseLump( i, lumpData, version );
			}
			catch ( Exception ex )
			{
				Log.Error( $"Failed decompiling lump: {i} {ex}" );
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
