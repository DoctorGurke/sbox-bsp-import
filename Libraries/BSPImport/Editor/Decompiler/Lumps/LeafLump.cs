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
			short packed = leafReader.ReadInt16();

			short area = (short)(packed & 0x01FF);      // 9 bits
			short flags = (short)((packed >> 9) & 0x7F);  // 7 bits

			leafReader.Skip<short>( 3 ); // mins
			leafReader.Skip<short>( 3 ); // maxs
			ushort firstLeafFace = leafReader.ReadUInt16();
			ushort leafFaceCount = leafReader.ReadUInt16();
			leafReader.Skip<ushort>(); // firstleafbrush
			leafReader.Skip<ushort>(); // numleafbrushes
			short leafWaterDataID = leafReader.ReadInt16();

			var leaf = new MapLeaf( contents, area, flags, firstLeafFace, leafFaceCount, leafWaterDataID );
			leafs[i] = leaf;
		}

		Context.Leafs = leafs;
	}
}

[Flags]
public enum ContentsFlags : int
{
	Empty = 0,
	Solid = 1,
	Window = 2,
	Aux = 4,
	Grate = 8,
	Slime = 16,
	Water = 32,
	BlockLOS = 64,
	Opaque = 128,
	TestFogVolume = 256,
	Unused = 512,
	Unused6 = 1024,
	Team1 = 2048,
	Team2 = 4096,
	IgnoreNoDrawOpaque = 8192,
	Moveable = 16384,
	AreaPortal = 32768,
	PlayerClip = 65536,
	MonsterClip = 131072,
	Current_0 = 262144,
	Current_90 = 524288,
	Current_180 = 1048576,
	Current_270 = 2097152,
	Current_Up = 4194304,
	Current_Down = 8388608,
	Origin = 16777216,
	Monster = 33554432,
	Debris = 67108864,
	Detail = 134217728,
	Translucent = 268435456,
	Ladder = 536870912,
	Hitbox = 1073741824
}

public struct MapLeaf
{
	public ContentsFlags Contents;
	public short Area; // 9 bits
	public short Flags; // 7 bits
	public ushort FirstFaceIndex;
	public ushort FaceCount;
	public short WaterDataIndex;

	public MapLeaf( int contents, short area, short flags, ushort firstFaceIndex, ushort faceCount, short waterDataID )
	{
		Contents = (ContentsFlags)contents;
		Area = area;
		Flags = flags;
		FirstFaceIndex = firstFaceIndex;
		FaceCount = faceCount;
		WaterDataIndex = waterDataID;
	}
}
