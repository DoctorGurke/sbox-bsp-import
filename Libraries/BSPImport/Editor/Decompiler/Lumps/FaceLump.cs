namespace BspImport.Decompiler.Lumps;

public class FaceLump : BaseLump
{
	public FaceLump( ImportContext context, byte[] data, int version = 0 ) : base( context, data, version ) { }

	protected override void Parse( BinaryReader reader )
	{
		var structReaders = Context.FormatDescriptor.StructReaders;

		// Use the format descriptor's face size, e.g. 56 bytes for v20+, 104 for VTMB v17.
		var faceCount = reader.GetLength() / structReaders.FaceStructSize;
		var faces = new Face[faceCount];

		for ( int i = 0; i < faceCount; i++ )
		{
			faces[i] = structReaders.ReadFace( reader );
		}

		//Log.Info( $"FACES: {faces.Count()}" );

		Context.Geometry.SetFaces( faces );
	}
}

public struct Face
{
	public int FirstEdge;
	public short EdgeCount;
	public short TexInfo;
	public short DisplacementInfo;
	public float Area;
	public int OriginalFaceIndex;

	public Face( int firstEdge, short edgeCount, short texInfo, short dispInfo, float area, int oFace )
	{
		FirstEdge = firstEdge;
		EdgeCount = edgeCount;
		TexInfo = texInfo;
		DisplacementInfo = dispInfo;
		Area = area;
		OriginalFaceIndex = oFace;
	}

	/// <summary>
	/// Parses the texture name from a texInfo index.
	/// </summary>
	/// <param name="context">The Context to take the texInfo etc. from.</param>
	/// <returns>The name of the texture taken from context.TexDataStringData.</returns>
	public string? GetFaceMaterial( ImportContext context )
	{
		// get texture/material for face
		var texData = context.TexInfo?[TexInfo].TexData;

		if ( texData is null )
			return null;

		var stringTableIndex = context.TexDataStringTable?[texData.Value];

		if ( stringTableIndex is null )
			return null;

		return context.TexDataStringData.FromStringTableIndex( stringTableIndex.Value ).ToLower();
	}
}
