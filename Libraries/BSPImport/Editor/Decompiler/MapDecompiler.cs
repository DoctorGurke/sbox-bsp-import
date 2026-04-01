namespace BspImport.Decompiler;

using Formats;

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
		var mapName = Path.GetFileNameWithoutExtension( Context.Name );
		Log.Info( $"Decompiling Context '{Context.Name}' (map '{mapName}')." );

		var reader = new BinaryReader( new MemoryStream( Context.Data ) );

		// read bsp header
		var ident = reader.ReadInt32();
		var mapversion = reader.ReadInt32();

		// detect format based on version, this will be used to determine how to read lumps as we go along
		Context.BspVersion = mapversion;
		Context.FormatDescriptor = BspFormatRegistry.DetectByVersion( mapversion );
		RefineFormatWithMapName( mapversion );

		// iterate all 64 possible lump headers
		for ( int i = 0; i < 64; i++ )
		{
			var lumpInfo = ReadLumpInfo( reader, i );

			// only attempt to decompile lumps we know about
			if ( !Enum.IsDefined( typeof( LumpType ), i ) )
			{
				continue;
			}

			// prepare lump data section
			byte[] lumpData = new byte[lumpInfo.Length];
			Array.Copy( Context.Data, lumpInfo.Offset, lumpData, 0, lumpInfo.Length );

			if ( !Enum.IsDefined( typeof( LumpType ), i ) )
				continue;

			var lumpType = (LumpType)i;

			BaseLump? lump = null;
			try
			{
				lump = ParseLump( lumpType, lumpData, lumpInfo.Version );
			}
			catch ( Exception ex )
			{
				Log.Error( $"Failed decompiling lump: {(LumpType)i} {ex}" );
				continue;
			}

			if ( lump is null )
			{
				Log.Info( $"Skipping unsupported lump {lumpType}." );
				continue;
			}

			// refine format from entity classnames after lump 0, resolves shared BSP versions
			if ( i == 0 )
			{
				RefineFormatFromEntities( mapversion );
			}

			// store in context after we're done with all lumps
			Context.Lumps[i] = lump;
		}

		var revision = reader.ReadInt32();

		Log.Info( $"Finished Decompiling: [ident: {ident} version: {mapversion} revision: {revision}]" );
	}

	/// <summary>
	/// Read lump header to get info about lump in BSP
	/// </summary>
	/// <param name="reader"></param>
	/// <param name="index"></param>
	/// <returns></returns>
	/// <exception cref="InvalidOperationException"></exception>
	private LumpInfo ReadLumpInfo( BinaryReader reader, int index)
	{
		int offset; // offset into bsp
		int length; // length in bytes
		int version; // lump versions can affect data structure

		switch ( Context.FormatDescriptor.LumpHeaderLayout )
		{
			case LumpHeaderLayout.Standard:
				offset = reader.ReadInt32();
				length = reader.ReadInt32();
				version = reader.ReadInt32();
				break;

			case LumpHeaderLayout.VersionFirst:
				version = reader.ReadInt32();
				offset = reader.ReadInt32();
				length = reader.ReadInt32();
				break;

			default:
				throw new InvalidOperationException( $"Unsupported lump header layout: " +
				                                     $"{Context.FormatDescriptor.LumpHeaderLayout}" );
		}

		reader.Skip( 4 ); // fourCC - unused
		return new LumpInfo( index, offset, length, version );
	}
}

public readonly record struct LumpInfo(int Index, int Offset, int Length, int Version)
{
	public bool IsEmpty => Offset == 0 || Length == 0;
}

public enum LumpHeaderLayout
{
	Standard,           // fileofs, filelen, version, fourCC
	VersionFirst        // version, fileofs, filelen, fourCC
}

public enum BrushSideLayout
{
	Standard,
	ThinFlag
}

public enum StaticPropLayout
{
	V4,
	V5,
	V6,
	V7,
	V7Xbox,
	V8,
	V9,
	V10,
	V11
}
