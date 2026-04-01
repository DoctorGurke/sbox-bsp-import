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

			// unpack flags, dont need area
			short packed = leafReader.ReadInt16();// area:9 flags:7
			short flags = (short)(packed >> 9);

			leafReader.Skip<short>( 3 ); // mins
			leafReader.Skip<short>( 3 ); // maxs
			ushort firstLeafFace = leafReader.ReadUInt16();
			ushort leafFaceCount = leafReader.ReadUInt16();
			leafReader.Skip<ushort>(); // firstleafbrush
			leafReader.Skip<ushort>(); // numleafbrushes

			var leaf = new MapLeaf( contents, flags, firstLeafFace, leafFaceCount );
			leafs[i] = leaf;
		}

		Context.Leafs = leafs;
	}
}

public struct MapLeaf
{
	public int Contents;
	public short Flags; // 7 bits
	public ushort FirstFaceIndex;
	public ushort FaceCount;

	public MapLeaf( int contents, short flags, ushort firstFaceIndex, ushort faceCount )
	{
		Contents = contents;
		Flags = flags;
		FirstFaceIndex = firstFaceIndex;
		FaceCount = faceCount;
	}
}
