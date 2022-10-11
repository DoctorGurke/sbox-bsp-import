namespace BspImport.Decompiler.Lumps;

public class OriginalFaceLump : BaseLump
{
	public OriginalFaceLump( DecompilerContext context, byte[] data, int version = 0 ) : base( context, data, version ) { }

	protected override void Parse( ByteParser data )
	{
		// each face is 56 bytes
		var faces = data.BufferCapacity / 56;

		var oFaces = new Face[faces];

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

			oFaces[i] = new Face( firstEdge, numEdges, texInfo, dispInfo, 0 );
		}

		Log.Info( $"ORIGINAL FACES: {oFaces.Count()}" );

		Context.MapGeometry.OriginalFaces = oFaces;
	}
}
