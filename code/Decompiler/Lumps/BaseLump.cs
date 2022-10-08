namespace BspImport.Decompiler.Lumps;

public abstract class BaseLump
{
	public IEnumerable<byte> Data { get; set; }
	public int Version { get; private set; }

	public BaseLump( IEnumerable<byte> data, int version = 0 )
	{
		Data = data;
		Version = version;
	}
}
