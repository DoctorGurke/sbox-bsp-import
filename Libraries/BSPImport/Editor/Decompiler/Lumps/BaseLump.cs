namespace BspImport.Decompiler.Lumps;

public abstract partial class BaseLump
{
	protected ImportContext Context { get; set; }
	public int Version { get; private set; }
	protected byte[] Data { get; private set; }

	public BaseLump( ImportContext context, byte[] data, int version = 0 )
	{
		if ( IsCompressed( data ) )
		{
			data = Decompress( data );
		}

		Context = context;
		Data = data;
		Version = version;

		var bReader = new BinaryReader( new MemoryStream( Data ) );
		Parse( bReader );
	}

	protected abstract void Parse( BinaryReader reader );
}
