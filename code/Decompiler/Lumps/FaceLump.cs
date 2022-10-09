namespace BspImport.Decompiler.Lumps;

public class FaceLump : BaseLump
{
	public FaceLump( DecompilerContext context, IEnumerable<byte> data, int version = 0 ) : base( context, data, version )
	{
		var parser = new ByteParser( data );

		// each face is 56 bytes
		var faces = parser.BufferCapacity / 56;

		var list = new List<Face>();

		for ( int i = 0; i < faces; i++ )
		{
			var faceparser = new ByteParser( parser.ReadBytes( 56 ) );
			faceparser.Skip<ushort>(); // planenum
			faceparser.Skip<byte>(); // side
			faceparser.Skip<byte>(); // onNode

			var firstedge = faceparser.Read<int>();
			var numedges = faceparser.Read<short>();
			var texinfo = faceparser.Read<short>();
			var dispinfo = faceparser.Read<short>();

			// don't need any of this
			faceparser.Skip<short>(); // surfaceFogVolumeID
			faceparser.Skip<byte>( 4 ); // styles[4]
			faceparser.Skip<int>(); // lightofs
			faceparser.Skip<float>(); // area
			faceparser.Skip<int>( 2 ); // LightmapTextureMinsInLuxels[2]
			faceparser.Skip<int>( 2 ); // LightmapTextureSizeInLuxels[2]

			var origface = faceparser.Read<int>();

			list.Add( new Face( firstedge, numedges, texinfo, dispinfo, origface ) );
		}

		Log.Info( $"FACES: {list.Count()}" );

		Context.MapGeometry.Faces = list;
	}
}

public struct Face
{
	public int FirstEdge;
	public short EdgeCount;
	public short TexInfo;
	public short DisplacementInfo;
	public int OriginalFaceIndex;

	public Face( int firstedge, short edgecount, short texinfo, short dispinfo, int origface )
	{
		FirstEdge = firstedge;
		EdgeCount = edgecount;
		TexInfo = texinfo;
		DisplacementInfo = dispinfo;
		OriginalFaceIndex = origface;
	}
}
