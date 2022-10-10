namespace BspImport.Decompiler.Lumps;

public class OriginalFaceLump : BaseLump
{
	public OriginalFaceLump( DecompilerContext context, IEnumerable<byte> data, int version = 0 ) : base( context, data, version ) { }

	protected override void Parse( ByteParser data )
	{
		// each face is 56 bytes
		var faces = data.BufferCapacity / 56;

		var list = new List<Face>();

		for ( int i = 0; i < faces; i++ )
		{
			var faceparser = new ByteParser( data.ReadBytes( 56 ) );
			faceparser.Skip<ushort>(); // planenum
			faceparser.Skip<byte>(); // side
			faceparser.Skip<byte>(); // onNode

			var firstEdge = faceparser.Read<int>();
			var numEdges = faceparser.Read<short>();
			var texInfo = faceparser.Read<short>();
			var dispInfo = faceparser.Read<short>();

			list.Add( new Face( firstEdge, numEdges, texInfo, dispInfo, 0 ) );
		}

		Log.Info( $"ORIGINAL FACES: {list.Count()}" );

		Context.MapGeometry.OriginalFaces = list;
	}
}
