using BspImport.Extensions;
using Sandbox;
using System.Runtime.InteropServices;

namespace BspImport.Decompiler.Lumps;

public class TexInfoLump : BaseLump
{
	public TexInfoLump( ImportContext context, byte[] data, int version = 0 ) : base( context, data, version ) { }

	protected override void Parse( BinaryReader reader )
	{
		var texInfoCount = reader.GetLength() / 72;

		var texInfos = new TexInfo[texInfoCount];

		for ( int i = 0; i < texInfoCount; i++ )
		{
			var tv0 = reader.ReadVector4();
			var tv1 = reader.ReadVector4();
			reader.Skip<Vector4>( 2 ); // vec4 * 2 : lightmapVecs[2][4]
			reader.Skip<int>(); // int flags
			var texData = reader.ReadInt32();

			var texInfo = new TexInfo( tv0, tv1, texData );
			texInfos[i] = texInfo;
		}

		Log.Info( $"TEXINFO: {texInfos.Length}" );

		Context.TexInfo = texInfos;
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

	// taken from ata4/bspsrc @ github
	public Vector2 GetUvs( Vector3 origin, int width, int height )
	{
		var tv0 = new Vector3( TextureVecs[0] );
		var tv1 = new Vector3( TextureVecs[1] );

		var u = tv0.x * origin.x + tv0.y * origin.y + tv0.z * origin.z + TextureVecs[0].w;
		var v = tv1.x * origin.x + tv1.y * origin.y + tv1.z * origin.z + TextureVecs[1].w;

		u /= width;
		v /= height;

		return new Vector2( u, v );
	}
}
