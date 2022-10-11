namespace BspImport.Decompiler.Lumps;

public class FaceLump : BaseLump
{
	public FaceLump( DecompilerContext context, byte[] data, int version = 0 ) : base( context, data, version ) { }

	protected override void Parse( ByteParser data )
	{
		var bReader = new BinaryReader( new MemoryStream( data ) );

		// each face is 56 bytes
		var faceCount = data.BufferCapacity / 56;

		var faces = new Face[faceCount];

		for ( int i = 0; i < faceCount; i++ )
		{
			var faceParser = new BinaryReader( new MemoryStream( bReader.ReadBytes( 56 ) ) );
			faceParser.ReadUInt16(); // planenum
			faceParser.ReadByte(); // side
			faceParser.ReadByte(); // onNode

			var firstEdge = faceParser.ReadInt32();
			var numEdges = faceParser.ReadInt16();
			var texInfo = faceParser.ReadInt16();
			var dispInfo = faceParser.ReadInt16();

			// don't need any of this
			faceParser.ReadInt16(); // short surfaceFogVolumeID
			faceParser.ReadBytes( 4 ); // byte styles[4]
			faceParser.ReadInt32(); // int lightofs
			faceParser.ReadSingle(); // float area
			faceParser.ReadBytes( sizeof( int ) * 2 ); // int LightmapTextureMinsInLuxels[2]
			faceParser.ReadBytes( sizeof( int ) * 2 ); // int LightmapTextureSizeInLuxels[2]

			var oFace = faceParser.ReadInt32();

			faces[i] = new Face( firstEdge, numEdges, texInfo, dispInfo, oFace );
		}

		Log.Info( $"FACES: {faces.Count()}" );

		Context.MapGeometry.Faces = faces;
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
