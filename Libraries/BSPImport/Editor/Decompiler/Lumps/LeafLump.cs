namespace BspImport.Decompiler.Lumps;

public class LeafLump : BaseLump
{
	public LeafLump( ImportContext context, byte[] data, int version = 0 ) : base( context, data, version ) { }

	protected override void Parse( BinaryReader reader )
	{
		var leafSize = 32; // 32 bytes per leaf
		var leafCount = reader.GetLength() / leafSize;

		var leafs = new MapLeaf[leafCount];

		for ( int i = 0; i < leafCount; i++ )
		{
			var leafReader = reader.Split( leafSize );

			int contents = leafReader.ReadInt32(); // contents
			leafReader.Skip<short>(); // cluster
			leafReader.Skip<short>(); // area:9 flags:7
			leafReader.Skip<short>( 3 ); // mins
			leafReader.Skip<short>( 3 ); // maxs
			ushort firstLeafFace = leafReader.ReadUInt16();
			ushort leafFaceCount = leafReader.ReadUInt16();

			var leaf = new MapLeaf( contents, firstLeafFace, leafFaceCount );
			leafs[i] = leaf;
		}

		Context.Leafs = leafs;
	}
}

public struct MapLeaf
{
	public int Contents;
	public ushort FirstFaceIndex;
	public ushort FaceCount;

	public MapLeaf( int contents, ushort firstFaceIndex, ushort faceCount )
	{
		Contents = contents;
		FirstFaceIndex = firstFaceIndex;
		FaceCount = faceCount;
	}
}
