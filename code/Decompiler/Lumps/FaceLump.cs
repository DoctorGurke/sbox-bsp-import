using BspImport.Extensions;

namespace BspImport.Decompiler.Lumps;

public class FaceLump : BaseLump
{
	public FaceLump( DecompilerContext context, byte[] data, int version = 0 ) : base( context, data, version ) { }

	protected override void Parse( BinaryReader reader, int capacity )
	{
		// each face is 56 bytes
		var faceCount = capacity / 56;

		var faces = new Face[faceCount];

		for ( int i = 0; i < faceCount; i++ )
		{
			faces[i] = reader.ReadFace();
		}

		Log.Info( $"FACES: {faces.Count()}" );

		Context.MapGeometry.Faces = faces;
	}
}

public struct Face
{
	public int FirstEdge;
	public short EdgeCount;
	public short TexInfo;
	public short DisplacementInfo;
	public int OriginalFaceIndex;

	public Face( int firstEdge, short edgeCount, short texInfo, short dispInfo, int oFace )
	{
		FirstEdge = firstEdge;
		EdgeCount = edgeCount;
		TexInfo = texInfo;
		DisplacementInfo = dispInfo;
		OriginalFaceIndex = oFace;
	}
}
