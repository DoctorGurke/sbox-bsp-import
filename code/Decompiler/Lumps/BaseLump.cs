namespace BspImport.Decompiler.Lumps;

public abstract class BaseLump
{
	protected DecompilerContext Context { get; set; }
	public int Version { get; private set; }

	public BaseLump( DecompilerContext context, IEnumerable<byte> data, int version = 0 )
	{
		Context = context;
		Version = version;

		var parser = new ByteParser( data );
		Parse( parser );
	}

	protected abstract void Parse( ByteParser data );
}
