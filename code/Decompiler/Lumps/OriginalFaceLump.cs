namespace BspImport.Decompiler.Lumps;

public class OriginalFaceLump : BaseLump
{
	public OriginalFaceLump( DecompilerContext context, IEnumerable<byte> data, int version = 0 ) : base( context, data, version ) { }

	protected override void Parse( IEnumerable<byte> data )
	{
		var parser = new ByteParser( data );

		// each face is 56 bytes
		var faces = parser.BufferCapacity / 56;

		var list = new List<OriginalFace>();

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

			list.Add( new OriginalFace( firstedge, numedges, texinfo, dispinfo ) );
		}

		Log.Info( $"ORIGINAL FACES: {list.Count()}" );

		Context.MapGeometry.OriginalFaces = list;
	}
}

public struct OriginalFace
{
	public int FirstEdge;
	public short EdgeCount;
	public short TexInfo;
	public short DisplacementInfo;

	public OriginalFace( int firstedge, short edgecount, short texinfo, short dispinfo )
	{
		FirstEdge = firstedge;
		EdgeCount = edgecount;
		TexInfo = texinfo;
		DisplacementInfo = dispinfo;
	}
}
