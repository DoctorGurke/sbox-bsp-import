namespace BspImport.Decompiler.Lumps;

public class OriginalFaceLump : BaseLump
{
	public OriginalFaceLump( ImportContext context, byte[] data, int version = 0 ) : base( context, data, version ) { }

	protected override void Parse( BinaryReader reader )
	{
		var structReaders = Context.FormatDescriptor.StructReaders;

		// each face is different for each format, so we need to read them all in one go
		var oFaceCount = reader.GetLength() / structReaders.FaceStructSize;
		var oFaces = new Face[oFaceCount];

		for ( var i = 0; i < oFaceCount; i++ )
		{
			oFaces[i] = structReaders.ReadFace( reader );
		}

		//Log.Info( $"ORIGINAL FACES: {oFaces.Count()}" );

		Context.Geometry.SetOriginalFaces( oFaces );
	}
}
