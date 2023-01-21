namespace BspImport.Decompiler.Lumps;

public class NodeLump : BaseLump
{
	public NodeLump( ImportContext context, byte[] data, int version = 0 ) : base( context, data, version ) { }

	protected override void Parse( BinaryReader reader )
	{
		var nodeSize = 32;// 32 bytes per node
		var nodeCount = reader.GetLength() / nodeSize;

		Log.Info( $"Parsing {nodeCount} nodes..." );

		var nodes = new MapNode[nodeCount];

		for ( int i = 0; i < nodeCount; i++ )
		{
			var nodeReader = reader.Split( nodeSize );

			nodeReader.Skip<int>(); // int planenum
			var firstChildIndex = nodeReader.ReadInt32();
			var secondChildIndex = nodeReader.ReadInt32();
			nodeReader.Skip<short>( 3 ); // mins
			nodeReader.Skip<short>( 3 ); // maxs
			var firstFaceIndex = nodeReader.ReadUInt16();
			var faceCount = nodeReader.ReadUInt16();

			Log.Info( $"parsed node: {firstChildIndex} {secondChildIndex} {firstFaceIndex} {faceCount}" );
			var node = new MapNode( firstChildIndex, secondChildIndex, firstFaceIndex, faceCount );
			nodes[i] = node;
		}

		Log.Info( $"NODES: {nodes.Length}" );
		Context.Nodes = nodes;
	}
}

public struct MapNode
{
	public int FirstChildIndex;
	public int SecondChildIndex;
	public ushort FirstFaceIndex;
	public ushort FaceCount;

	public MapNode( int firstChildIndex, int secondChildIndex, ushort firstFaceIndex, ushort faceCount )
	{
		FirstChildIndex = firstChildIndex;
		SecondChildIndex = secondChildIndex;
		FirstFaceIndex = firstFaceIndex;
		FaceCount = faceCount;
	}
}
