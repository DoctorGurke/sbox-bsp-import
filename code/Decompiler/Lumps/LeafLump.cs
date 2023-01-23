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

			leafReader.Skip<int>(); // contents
			leafReader.Skip<short>(); // cluster
			leafReader.Skip<short>(); // area:9 flags:7
			leafReader.Skip<short>( 3 ); // mins
			leafReader.Skip<short>( 3 ); // maxs
			var firstLeafFace = (int)leafReader.ReadUInt16();
			var leafFaceCount = (int)leafReader.ReadUInt16();

			var leaf = new MapLeaf( firstLeafFace, leafFaceCount );
			leafs[i] = leaf;
		}

		Log.Info( $"LEAFS: {leafCount}" );
		Context.Leafs = leafs;
	}
}

public struct MapLeaf
{
	public int FirstFaceIndex;
	public int FaceCount;

	public MapLeaf( int firstFaceIndex, int faceCount )
	{
		FirstFaceIndex = firstFaceIndex;
		FaceCount = faceCount;
	}
}
