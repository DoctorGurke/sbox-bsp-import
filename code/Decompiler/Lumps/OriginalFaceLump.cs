using BspImport.Extensions;

namespace BspImport.Decompiler.Lumps;

public class OriginalFaceLump : BaseLump
{
	public OriginalFaceLump( DecompilerContext context, byte[] data, int version = 0 ) : base( context, data, version ) { }

	protected override void Parse( BinaryReader reader, int capacity )
	{
		// each face is 56 bytes
		var oFaceCount = capacity / 56;

		var oFaces = new Face[oFaceCount];

		for ( int i = 0; i < oFaceCount; i++ )
		{
			oFaces[i] = reader.ReadFace();
		}

		Log.Info( $"ORIGINAL FACES: {oFaces.Count()}" );

		Context.MapGeometry.OriginalFaces = oFaces;
	}
}
