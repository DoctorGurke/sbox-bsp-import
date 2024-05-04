using BspImport.Extensions;

namespace BspImport.Decompiler.Lumps;

public class TexDataLump : BaseLump
{
	public TexDataLump( ImportContext context, byte[] data, int version = 0 ) : base( context, data, version ) { }

	protected override void Parse( BinaryReader reader )
	{
		var texDataCount = reader.GetLength() / 32;

		var texDatas = new TexData[texDataCount];

		for ( int i = 0; i < texDataCount; i++ )
		{
			reader.Skip<Vector3>(); // vec3 reflectivity
			var nameStringTableID = reader.ReadInt32();
			var width = reader.ReadInt32();
			var height = reader.ReadInt32();
			reader.Skip<int>( 2 ); // int view_width, view_height

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
