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
	}

	public void Parse()
	{
		var bReader = new BinaryReader( new MemoryStream( Data ) );
		Parse( bReader );
	}

	protected abstract void Parse( BinaryReader reader );
}
