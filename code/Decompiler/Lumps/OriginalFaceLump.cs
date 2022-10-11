namespace BspImport.Decompiler.Lumps;

public class OriginalFaceLump : BaseLump
{
	public OriginalFaceLump( DecompilerContext context, byte[] data, int version = 0 ) : base( context, data, version ) { }

	protected override void Parse( ByteParser data )
	{
		var bReader = new BinaryReader( new MemoryStream( data ) );

		// each face is 56 bytes
		var oFaceCount = data.BufferCapacity / 56;

		var oFaces = new Face[oFaceCount];

		for ( int i = 0; i < oFaceCount; i++ )
		{
			var faceParser = new BinaryReader( new MemoryStream( bReader.ReadBytes( 56 ) ) );
			faceParser.ReadUInt16(); // planenum
			faceParser.ReadByte(); // side
			faceParser.ReadByte(); // onNode

			var firstEdge = faceParser.ReadInt32();
			var numEdges = faceParser.ReadInt16();
			var texInfo = faceParser.ReadInt16();
			var dispInfo = faceParser.ReadInt16();

			oFaces[i] = new Face( firstEdge, numEdges, texInfo, dispInfo, 0 );
		}

		Log.Info( $"ORIGINAL FACES: {oFaces.Count()}" );

		Context.MapGeometry.OriginalFaces = oFaces;
	}
}
