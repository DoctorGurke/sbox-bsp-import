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

			var minTess = rInfo.ReadInt32();
			var smoothingAngle = rInfo.ReadSingle();
			rInfo.Skip<int>(); // contents

			var mapFace = rInfo.ReadUInt16();

			rInfo.Skip<int>(); // lightmapAlphaStart
			rInfo.Skip<int>(); // lightmapSamplePositionStart

			var info = new DisplacementInfo( startPosition, firstVertex, firstTri, power, minTess, smoothingAngle, mapFace );
			infos[i] = info;

			var remaining = rInfo.GetLength();
			Log.Info( $"remaining: {remaining}" );

		}

		Context.Geometry.SetDisplacementInfos( infos );
	}
}

public struct DisplacementInfo
{
	public Vector3 StartPosition;
	public int FirstVertex;
	public int FirstTri;
	public int Power;
	public int MinTess;
	public float SmoothingAngle;
	public ushort MapFace;

	// helper: convert power level to side length

	public DisplacementInfo( Vector3 startPosition, int firstVertex, int firstTri, int power, int minTess, float smoothingAngle, ushort mapFace )
	{
		StartPosition = startPosition;
		FirstVertex = firstVertex;
		FirstTri = firstTri;
		Power = power;
		MinTess = minTess;
		SmoothingAngle = smoothingAngle;
		MapFace = mapFace;
	}

	public int Side => (1 << Power) + 1;
	public int VertCount => Side * Side;
}
