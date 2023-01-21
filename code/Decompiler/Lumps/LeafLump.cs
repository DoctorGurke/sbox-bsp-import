namespace BspImport.Decompiler.Lumps;

public class LeafLump : BaseLump
{
	public LeafLump( ImportContext context, byte[] data, int version = 0 ) : base( context, data, version ) { }

	protected override void Parse( BinaryReader reader )
	{
		//throw new NotImplementedException();
	}
}
