namespace BspImport.Decompiler.Lumps;

public class TexInfoLump : BaseLump
{
	public TexInfoLump( DecompilerContext context, IEnumerable<byte> data, int version = 0 ) : base( context, data, version ) { }

	protected override void Parse( ByteParser data )
	{
		var list = new List<TexInfo>();

		var texInfoCount = data.BufferCapacity / 72;

		for ( int i = 0; i < texInfoCount; i++ )
		{
			var tv0 = data.Read<Vector4>();
			var tv1 = data.Read<Vector4>();
			data.Skip<Vector4>( 2 ); // lightmapVecs[2][4]
			data.Skip<int>(); // flags
			var texData = data.Read<int>();

			var texInfo = new TexInfo( tv0, tv1, texData );
			list.Add( texInfo );
		}

		Log.Info( $"TEXINFO: {list.Count()}" );

		Context.TexInfo = list;
	}
}

public struct TexInfo
{
	public Vector4[] TextureVecs;
	public int TexData;

	public TexInfo( Vector4 tv0, Vector4 tv1, int texData )
	{
		TextureVecs = new Vector4[2];
		TextureVecs[0] = tv0;
		TextureVecs[1] = tv1;
		TexData = texData;
	}
}
