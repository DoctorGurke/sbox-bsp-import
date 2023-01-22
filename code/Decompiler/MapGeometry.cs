namespace BspImport.Decompiler;

public class MapGeometry
{
	public Vector3[]? Vertices { get; set; }
	public EdgeIndices[]? EdgeIndices { get; set; }
	public int[]? SurfaceEdges { get; set; }
	public ushort[]? LeafFaceIndices { get; set; }
	public Face[]? Faces { get; set; }
	public Face[]? OriginalFaces { get; set; }
	public DisplacementVertex[]? DisplacementVertices { get; set; }
	public DisplacementInfo[]? DisplacementInfos { get; set; }
}
