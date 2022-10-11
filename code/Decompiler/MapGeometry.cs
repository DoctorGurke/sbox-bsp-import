using BspImport.Decompiler.Lumps;

namespace BspImport.Decompiler;

public class MapGeometry
{
	public Vector3[]? VertexPositions { get; set; }
	public EdgeIndices[]? EdgeIndices { get; set; }
	public int[]? SurfaceEdges { get; set; }
	public Face[]? Faces { get; set; }
	public Face[]? OriginalFaces { get; set; }

}
