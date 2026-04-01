namespace BspImport.Decompiler;

public class MapGeometry
{
	public bool IsValid()
	{
		return VertexCount > 0
			&& EdgeIndicesCount > 0
			&& SurfaceEdgesCount > 0
			&& LeafFaceIndicesCount > 0
			&& FacesCount > 0
			&& OriginalFaceCount > 0;
	}

	/// 
	/// Vertices
	/// 

	private Vector3[] Vertices = Array.Empty<Vector3>();
	public int VertexCount => Vertices.Length;
	public void SetVertices( ReadOnlySpan<Vector3> span ) => Vertices = span.ToArray();
	public bool TryGetVertex( int index, out Vector3 vertex )
	{
		if ( index >= 0 && index < Vertices.Length )
		{
			vertex = Vertices[index];
			return true;
		}

		vertex = default;
		return false;
	}


	/// 
	/// Edge Indices
	/// 

	private EdgeIndices[] EdgeIndices = Array.Empty<EdgeIndices>();
	public int EdgeIndicesCount => EdgeIndices.Length;
	public void SetEdgeIndices( ReadOnlySpan<EdgeIndices> span ) => EdgeIndices = span.ToArray();
	public bool TryGetEdgeIndices( int index, out EdgeIndices edgeIndices )
	{
		if ( index >= 0 && index < EdgeIndices.Length )
		{
			edgeIndices = EdgeIndices[index];
			return true;
		}

		edgeIndices = default!;
		return false;
	}

	/// 
	/// Surface Edges
	/// 

	private int[] SurfaceEdges = Array.Empty<int>();
	public int SurfaceEdgesCount => SurfaceEdges.Length;
	public void SetSurfaceEdges( ReadOnlySpan<int> span ) => SurfaceEdges = span.ToArray();
	public bool TryGetSurfaceEdge( int index, out int surfEdge )
	{
		if ( index >= 0 && index < SurfaceEdges.Length )
		{
			surfEdge = SurfaceEdges[index];
			return true;
		}

		surfEdge = 0;
		return false;
	}

	/// 
	/// Faces
	/// 

	private Face[] Faces = Array.Empty<Face>();
	public int FacesCount => Faces.Length;
	public void SetFaces( ReadOnlySpan<Face> span ) => Faces = span.ToArray();
	public bool TryGetFace( ushort index, out Face face )
	{
		if ( index >= 0 && index < Faces.Length )
		{
			face = Faces[index];
			return true;
		}

		face = default!;
		return false;
	}

	/// 
	/// Original Faces
	/// 

	private Face[] OriginalFaces = Array.Empty<Face>();
	public int OriginalFaceCount => OriginalFaces.Length;
	public void SetOriginalFaces( ReadOnlySpan<Face> span ) => OriginalFaces = span.ToArray();
	public bool TryGetOriginalFace( int index, out Face face )
	{
		if ( index >= 0 && index < OriginalFaces.Length )
		{
			face = OriginalFaces[index];
			return true;
		}

		face = default!;
		return false;
	}

	/// 
	/// Leaf Face Indices
	/// 

	private ushort[] LeafFaceIndices = Array.Empty<ushort>();
	public int LeafFaceIndicesCount => LeafFaceIndices.Length;
	public void SetLeafFaceIndices( ReadOnlySpan<ushort> span ) => LeafFaceIndices = span.ToArray();
	public bool TryGetLeafFaceIndex( int index, out ushort value )
	{
		if ( index >= 0 && index < LeafFaceIndices.Length )
		{
			value = LeafFaceIndices[index];
			return true;
		}

		value = 0;
		return false;
	}

	/// 
	/// Displacement Vertices
	/// 

	private DisplacementVertex[] DisplacementVertices = Array.Empty<DisplacementVertex>();
	public int DisplacementVertexCount => DisplacementVertices.Length;
	public void SetDisplacementVertices( ReadOnlySpan<DisplacementVertex> span ) => DisplacementVertices = span.ToArray();
	public bool TryGetDisplacementVertex( int index, out DisplacementVertex dv )
	{
		if ( index >= 0 && index < DisplacementVertices.Length )
		{
			dv = DisplacementVertices[index];
			return true;
		}

		dv = default!;
		return false;
	}

	/// 
	/// Displacement Infos
	/// 

	private DisplacementInfo[] DisplacementInfos = Array.Empty<DisplacementInfo>();
	public int DisplacementInfoCount => DisplacementInfos.Length;
	public void SetDisplacementInfos( ReadOnlySpan<DisplacementInfo> span ) => DisplacementInfos = span.ToArray();
	public bool TryGetDisplacementInfo( short index, out DisplacementInfo info )
	{
		if ( index >= 0 && index < DisplacementInfos.Length )
		{
			info = DisplacementInfos[index];
			return true;
		}

		info = default!;
		return false;
	}
}
