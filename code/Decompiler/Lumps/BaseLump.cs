namespace BspImport.Decompiler.Lumps;

public abstract class BaseLump
{
	protected ImportContext Context { get; set; }
	public int Version { get; private set; }
	protected byte[] Data { get; private set; }

	public BaseLump( ImportContext context, byte[] data, int version = 0 )
	{
		Context = context;
		Data = data;
		Version = version;

		CheckDecompress( data );

		var bReader = new BinaryReader( new MemoryStream( Data ) );
		Parse( bReader );
	}

	private const uint LZMA_ID = (('A' << 24) | ('M' << 16) | ('Z' << 8) | ('L'));

	private void CheckDecompress( byte[] data )
	{
		var reader = new BinaryReader( new MemoryStream( data ) );
		var lzmaId = reader.ReadUInt32();

		if ( lzmaId == LZMA_ID )
			Log.Warning( $"Lump {this.GetType()} is compressed!" );
	}

	protected abstract void Parse( BinaryReader reader );
}
