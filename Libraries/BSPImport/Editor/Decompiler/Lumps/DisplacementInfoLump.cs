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

			var info = new DisplacementInfo( startPosition, firstVertex, firstTri, power );
			infos[i] = info;
		}

		Log.Info( $"DISPLACEMENT INFOS: {infos.Length}" );

		Context.Geometry.DisplacementInfos = infos;
	}
}

public struct DisplacementInfo
{
	public Vector3 StartPosition;
	public int FirstVertex;
	public int FirstTri;
	public int Power;

	public DisplacementInfo( Vector3 startPosition, int firstVertex, int firstTri, int power )
	{
		StartPosition = startPosition;
		FirstVertex = firstVertex;
		FirstTri = firstTri;
		Power = power;
	}
}
