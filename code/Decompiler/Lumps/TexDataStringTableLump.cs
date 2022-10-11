namespace BspImport.Decompiler.Lumps;

public class TexDataStringTableLump : BaseLump
{
	public TexDataStringTableLump( DecompilerContext context, byte[] data, int version = 0 ) : base( context, data, version ) { }

	protected override void Parse( ByteParser data )
	{
		var indices = data.TryReadMultiple<int>();

		Log.Info( $"TEXDATASTRINGTABLE: {indices.Count()}" );

		Context.TexDataStringTable = indices;
	}
}
