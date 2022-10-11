using BspImport.Extensions;
using System.Runtime.InteropServices;

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

	// taken from ata4/bspsrc @ github
	public Vector2 GetUvs( Vector3 origin, Angles angles, int width, int height )
	{
		var uaxis = new Vector3( TextureVecs[0] );
		var vaxis = new Vector3( TextureVecs[1] );

		float utw = 1.0f / uaxis.Length;
		float vtw = 1.0f / vaxis.Length;

		uaxis *= utw;
		vaxis *= vtw;

		float ushift = TextureVecs[0].w;
		float vshift = TextureVecs[1].w;

		// translate to origin
		if ( !origin.AlmostEqual( Vector3.Zero ) )
		{
			ushift -= origin.Dot( uaxis ) / utw;
			vshift -= origin.Dot( vaxis ) / vtw;
		}

		// rotate texture
		if ( !angles.AlmostEqual( Angles.Zero ) )
		{
			var rotation = angles.ToRotation();

			uaxis *= rotation;
			vaxis *= rotation;

			// calculate shift uv space due to the rotation
			var shift = Vector3.Zero;
			shift -= origin;
			shift *= rotation;
			shift += origin;

			ushift -= shift.Dot( uaxis ) / utw;
			vshift -= shift.Dot( vaxis ) / vtw;
		}

		ushift /= width;
		vshift /= height;

		return new Vector2( ushift, vshift );
	}
}
