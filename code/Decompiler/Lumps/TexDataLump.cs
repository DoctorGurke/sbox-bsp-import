namespace BspImport.Decompiler.Lumps;

public class TexDataLump : BaseLump
{
	public TexDataLump( DecompilerContext context, byte[] data, int version = 0 ) : base( context, data, version ) { }

	protected override void Parse( ByteParser data )
	{
		var bReader = new BinaryReader( new MemoryStream( data ) );

		var texDataCount = data.BufferCapacity / 32;

		var texDatas = new TexData[texDataCount];

		for ( int i = 0; i < texDataCount; i++ )
		{
			bReader.ReadBytes( sizeof( float ) * 3 ); // vec3 reflectivity
			var nameStringTableID = bReader.ReadInt32();
			var width = bReader.ReadInt32();
			var height = bReader.ReadInt32();
			bReader.ReadBytes( sizeof( int ) * 2 ); // int view_width, view_height

			var texData = new TexData( nameStringTableID, width, height );
			texDatas[i] = texData;
		}

		Log.Info( $"TEXDATA: {texDatas.Length}" );

		Context.TexData = texDatas;
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
