namespace BspImport.Decompiler.Lumps;

public class NodeLump : BaseLump
{
	public NodeLump( ImportContext context, byte[] data, int version = 0 ) : base( context, data, version ) { }

	protected override void Parse( BinaryReader reader )
	{
		var nodeSize = 32;// 32 bytes per node
		var nodeCount = reader.GetLength() / nodeSize;

		var nodes = new MapNode[nodeCount];

		for ( int i = 0; i < nodeCount; i++ )
		{
			var nodeReader = reader.Split( nodeSize );

			int planeIndex = nodeReader.ReadInt32();
			int firstChildIndex = nodeReader.ReadInt32();
			int secondChildIndex = nodeReader.ReadInt32();
			nodeReader.Skip<short>( 3 ); // mins
			nodeReader.Skip<short>( 3 ); // maxs
			ushort firstFaceIndex = nodeReader.ReadUInt16();
			ushort faceCount = nodeReader.ReadUInt16();
			short area = nodeReader.ReadInt16();

			var node = new MapNode( planeIndex, new int[] { firstChildIndex, secondChildIndex }, firstFaceIndex, faceCount, area );
			nodes[i] = node;
		}

		//Log.Info( $"NODES: {nodes.Length}" );
		Context.Nodes = nodes;
	}
}

public struct MapNode
{
	public int PlaneIndex;
	public int[] Children;
	public ushort FirstFaceIndex;
	public ushort FaceCount;
	public short Area;

	public MapNode( int planeIndex, int[] childIndices, ushort firstFaceIndex, ushort faceCount, short area )
	{
		PlaneIndex = planeIndex;
		Children = childIndices;
		FirstFaceIndex = firstFaceIndex;
		FaceCount = faceCount;
		Area = area;
	}
}
