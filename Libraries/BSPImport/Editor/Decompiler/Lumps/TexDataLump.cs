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
			var reflectivity = reader.ReadVector3();
			var nameStringTableID = reader.ReadInt32();
			var width = reader.ReadInt32();
			var height = reader.ReadInt32();
			reader.Skip<int>( 2 ); // int view_width, view_height

			var texData = new TexData( reflectivity, nameStringTableID, width, height );
			texDatas[i] = texData;
		}

		Context.TexData = texDatas;
	}
}

public struct TexData
{
	public Vector3 Reflectivity;
	public int NameStringTableIndex;
	public int Width;
	public int Height;

	public TexData( Vector3 reflectivity, int index, int width, int height )
	{
		Reflectivity = reflectivity;
		NameStringTableIndex = index;
		Width = width;
		Height = height;
	}
}
