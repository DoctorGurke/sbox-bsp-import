namespace BspImport.Decompiler.Lumps;

public class OriginalFaceLump : BaseLump
{
	public OriginalFaceLump( ImportContext context, byte[] data, int version = 0 ) : base( context, data, version ) { }

	protected override void Parse( BinaryReader reader )
	{
		// each face is 56 bytes
		var oFaceCount = reader.GetLength() / 56;

		var oFaces = new Face[oFaceCount];

		for ( int i = 0; i < oFaceCount; i++ )
		{
			oFaces[i] = reader.ReadFace();
		}

		Log.Info( $"ORIGINAL FACES: {oFaces.Count()}" );

		Context.Geometry.OriginalFaces = oFaces;
	}
}
