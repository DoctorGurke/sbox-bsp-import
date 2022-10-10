using BspImport.Decompiler.Lumps;

namespace BspImport.Decompiler;

public class MapGeometry
{
	public IEnumerable<Vector3>? VertexPositions { get; set; }
	public IEnumerable<EdgeIndices>? EdgeIndices { get; set; }
	public IEnumerable<int>? SurfaceEdges { get; set; }
	public IEnumerable<Face>? Faces { get; set; }
	public IEnumerable<Face>? OriginalFaces { get; set; }

}
