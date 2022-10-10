namespace BspImport.Decompiler.Lumps;

public class FaceLump : BaseLump
{
	public FaceLump( DecompilerContext context, IEnumerable<byte> data, int version = 0 ) : base( context, data, version ) { }

	protected override void Parse( ByteParser data )
	{
		// each face is 56 bytes
		var faces = data.BufferCapacity / 56;

		var list = new List<Face>();

		for ( int i = 0; i < faces; i++ )
		{
			var faceParser = new ByteParser( data.ReadBytes( 56 ) );
			faceParser.Skip<ushort>(); // planenum
			faceParser.Skip<byte>(); // side
			faceParser.Skip<byte>(); // onNode

			var firstEdge = faceParser.Read<int>();
			var numEdges = faceParser.Read<short>();
			var texInfo = faceParser.Read<short>();
			var dispInfo = faceParser.Read<short>();

			// don't need any of this
			faceParser.Skip<short>(); // surfaceFogVolumeID
			faceParser.Skip<byte>( 4 ); // styles[4]
			faceParser.Skip<int>(); // lightofs
			faceParser.Skip<float>(); // area
			faceParser.Skip<int>( 2 ); // LightmapTextureMinsInLuxels[2]
			faceParser.Skip<int>( 2 ); // LightmapTextureSizeInLuxels[2]

			var oFace = faceParser.Read<int>();

			list.Add( new Face( firstEdge, numEdges, texInfo, dispInfo, oFace ) );
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

	public Face( int firstEdge, short edgeCount, short texInfo, short dispInfo, int oFace )
	{
		FirstEdge = firstEdge;
		EdgeCount = edgeCount;
		TexInfo = texInfo;
		DisplacementInfo = dispInfo;
		OriginalFaceIndex = oFace;
	}
}
