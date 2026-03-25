namespace BspImport.Decompiler.Lumps;

public class DisplacementInfoLump : BaseLump
{
	public DisplacementInfoLump( ImportContext context, byte[] data, int version = 0 ) : base( context, data, version ) { }

	protected override void Parse( BinaryReader reader )
	{
		var infoLength = 176;
		var infoCount = reader.GetLength() / infoLength;

		var infos = new DisplacementInfo[infoCount];

		for ( int i = 0; i < infoCount; i++ )
		{
			var rInfo = reader.Split( infoLength );

			var startPosition = rInfo.ReadVector3();
			var firstVertex = rInfo.ReadInt32();
			var firstTri = rInfo.ReadInt32();
			var power = rInfo.ReadInt32();

			rInfo.Skip<int>(); // minTess
			rInfo.Skip<float>(); // smoothingAngle
			rInfo.Skip<int>(); // contents

			var mapFace = rInfo.ReadUInt16();

			rInfo.Skip<int>(); // lightmapAlphaStart
			rInfo.Skip<int>(); // lightmapSamplePositionStart

			var info = new DisplacementInfo( startPosition, firstVertex, firstTri, power, mapFace );
			infos[i] = info;

			// read 4 edge neighbors
			{
				var structReader = new StructReader<DispNeighbor>();
				for ( int edgeNeighborIndex = 0; edgeNeighborIndex < 4; edgeNeighborIndex++ )
				{
					var neighbor = structReader.Read( rInfo.ReadBytes( Marshal.SizeOf<DispNeighbor>() ) );
					info.EdgeNeighbors[edgeNeighborIndex] = neighbor;
				}
			}
			// read 4 corner neighbors
			{
				var structReader = new StructReader<DispCornerNeighbors>();
				for ( int cornerNeighborIndex = 0; cornerNeighborIndex < 4; cornerNeighborIndex++ )
				{
					var neighbor = structReader.Read( rInfo.ReadBytes( Marshal.SizeOf<DispCornerNeighbors>() ) );
					info.CornerNeighbors[cornerNeighborIndex] = neighbor;
				}
			}

			var remaining = rInfo.GetLength();
			Log.Info( $"remaining: {remaining}" );

		}

		Context.Geometry.SetDisplacementInfos( infos );
	}
}

public struct DispSubNeighbor
{
	public bool IsValid() => NeighborIndex != 0xFFFF;

	public ushort NeighborIndex;
	public byte NeighborOrientation;
	public byte Span;
	public byte NeighborSpan;

	public DispSubNeighbor( ushort neighborIndex, byte neighborOrientation, byte span, byte neighborSpan )
	{
		NeighborIndex = neighborIndex;
		NeighborOrientation = neighborOrientation;
		Span = span;
		NeighborSpan = neighborSpan;
	}
}

public struct DispNeighbor
{
	public bool IsValid() => SubNeighbors[0].IsValid() || SubNeighbors[1].IsValid();

	[MarshalAs( UnmanagedType.ByValArray, SizeConst = 2 )]
	public DispSubNeighbor[] SubNeighbors;
}

public struct DispCornerNeighbors
{
	[MarshalAs( UnmanagedType.ByValArray, SizeConst = 4 )]
	public ushort[] Neighbors;
	public int NeighborCount;

	public DispCornerNeighbors( ushort[] neighbors, int neighborCount )
	{
		for ( int i = 0; i < neighborCount && i < 4; i++ )
		{
			Neighbors[i] = neighbors[i];
		}
		NeighborCount = neighborCount;
	}
}

public struct DisplacementInfo
{
	public Vector3 StartPosition;
	public int FirstVertex;
	public int FirstTri;
	public int Power;
	public ushort MapFace;
	public DispNeighbor[] EdgeNeighbors = new DispNeighbor[4];
	public DispCornerNeighbors[] CornerNeighbors = new DispCornerNeighbors[4];
	public ulong[] AllowedVerts = new ulong[10];

	public DisplacementInfo( Vector3 startPosition, int firstVertex, int firstTri, int power, ushort mapFace )
	{
		StartPosition = startPosition;
		FirstVertex = firstVertex;
		FirstTri = firstTri;
		Power = power;
		MapFace = mapFace;
	}
}
