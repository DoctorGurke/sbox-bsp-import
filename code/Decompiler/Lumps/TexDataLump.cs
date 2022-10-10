namespace BspImport.Decompiler.Lumps;

public class TexDataLump : BaseLump
{
	public TexDataLump( DecompilerContext context, IEnumerable<byte> data, int version = 0 ) : base( context, data, version ) { }

	protected override void Parse( ByteParser data )
	{
		var texDatas = data.BufferCapacity / 32;

		var list = new List<TexData>();

		for ( int i = 0; i < texDatas; i++ )
		{
			data.Skip<Vector3>(); // reflectivity
			var nameStringTableID = data.Read<int>();
			var width = data.Read<int>();
			var height = data.Read<int>();
			data.Skip<int>( 2 ); // view_width, view_height

			var texData = new TexData( nameStringTableID, width, height );
			list.Add( texData );
		}

		Log.Info( $"TEXDATA: {list.Count()}" );

		Context.TexData = list;
	}
}

public struct TexData
{
	public int NameStringTableIndex;
	public int Width;
	public int Height;

	public TexData( int index, int width, int height )
	{
		NameStringTableIndex = index;
		Width = width;
		Height = height;
	}
}
