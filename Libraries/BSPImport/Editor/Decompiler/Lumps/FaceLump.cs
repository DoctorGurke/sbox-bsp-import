namespace BspImport.Decompiler.Lumps;

public class FaceLump : BaseLump
{
	public FaceLump( ImportContext context, byte[] data, int version = 0 ) : base( context, data, version ) { }

	protected override void Parse( BinaryReader reader )
	{
		// each face is 56 bytes
		var faceCount = reader.GetLength() / 56;

		var faces = new Face[faceCount];

		for ( int i = 0; i < faceCount; i++ )
		{
			faces[i] = reader.ReadFace();
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
	public string? GetMaterialName( ImportContext context )
	{
		if ( context.TexInfo is null || TexInfo < 0 || TexInfo >= context.TexInfo.Length )
			return null;

		// get texture/material for face
		var texData = context.TexInfo[TexInfo].TexData;
		if ( context.TexData is null || texData < 0 || texData >= context.TexData.Length )
			return null;

		var stringTableIndex = context.TexData[texData].NameStringTableIndex;
		if ( context.TexDataStringTable is null || stringTableIndex < 0 || stringTableIndex >= context.TexDataStringTable.Length )
			return null;

		var stringDataIndex = context.TexDataStringTable[stringTableIndex];
		return context.TexDataStringData.FromStringTableIndex( stringDataIndex ).ToLower();
	}

	public Vector3 GetReflectivity( ImportContext context )
	{
		var texDataIndex = context.TexInfo?[TexInfo].TexData;

		if ( texDataIndex is null )
			return Vector3.One;

		var texData = context.TexData?[texDataIndex.Value];

		if ( texData is null )
			return Vector3.One;

		return texData.Value.Reflectivity;
	}
}
