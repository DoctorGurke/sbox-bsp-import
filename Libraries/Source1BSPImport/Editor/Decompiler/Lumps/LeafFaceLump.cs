namespace BspImport.Decompiler.Lumps;

public class LeafFaceLump : BaseLump
{
	public LeafFaceLump( ImportContext context, byte[] data, int version = 0 ) : base( context, data, version ) { }

	protected override void Parse( BinaryReader reader )
	{
		var faceCount = reader.GetLength() / sizeof( ushort );

		var faces = new ushort[faceCount];

		for ( int i = 0; i < faceCount; i++ )
		{
			faces[i] = reader.ReadUInt16();
		}

		Log.Info( $"LEAF FACES: {faces.Length}" );
		Context.Geometry.LeafFaceIndices = faces;
	}
}
