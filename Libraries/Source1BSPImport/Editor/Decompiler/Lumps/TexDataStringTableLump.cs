using BspImport.Extensions;

namespace BspImport.Decompiler.Lumps;

public class TexDataStringTableLump : BaseLump
{
	public TexDataStringTableLump( ImportContext context, byte[] data, int version = 0 ) : base( context, data, version ) { }

	protected override void Parse( BinaryReader reader )
	{
		var indexCount = reader.GetLength() / sizeof( int );

		var indices = new int[indexCount];

		for ( int i = 0; i < indexCount; i++ )
		{
			indices[i] = reader.ReadInt32();
		}

		Log.Info( $"TEXDATASTRINGTABLE: {indices.Count()}" );

		Context.TexDataStringTable = indices;
	}
}
