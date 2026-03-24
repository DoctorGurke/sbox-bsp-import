using System;
using System.Linq;

namespace BspImport.Decompiler;

public class MapGeometry
{
    private Vector3[] Vertices = Array.Empty<Vector3>();
	private EdgeIndices[] EdgeIndices = Array.Empty<EdgeIndices>();
	private int[] SurfaceEdges = Array.Empty<int>();
    private ushort[] LeafFaceIndices = Array.Empty<ushort>();
	private Face[] Faces = Array.Empty<Face>();
	private Face[] OriginalFaces = Array.Empty<Face>();
	private DisplacementVertex[] DisplacementVertices = Array.Empty<DisplacementVertex>();
	private DisplacementInfo[] DisplacementInfos = Array.Empty<DisplacementInfo>();

    public int VertexCount => Vertices.Length;
	public int EdgeIndicesCount => EdgeIndices.Length;
	public int SurfaceEdgesCount => SurfaceEdges.Length;
	public int LeafFaceIndicesCount => LeafFaceIndices.Length;
	public int FacesCount => Faces.Length;
	public int OriginalFaceCount => OriginalFaces.Length;

	public bool IsValid()
	{
		return VertexCount> 0
			&& EdgeIndicesCount > 0
			&& SurfaceEdgesCount > 0
			&& LeafFaceIndicesCount > 0
			&& FacesCount > 0 
			&& OriginalFaceCount > 0;
	}

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

    public bool TryGetFace( int index, out Face face )
	{
		if ( index >= 0 && index < Faces.Length )
		{
			face = Faces[index];
			return true;
		}

		face = default!;
		return false;
	}

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

	public bool TryGetDisplacementInfo( int index, out DisplacementInfo info )
	{
		if ( index >= 0 && index < DisplacementInfos.Length )
		{
			info = DisplacementInfos[index];
			return true;
		}

		info = default!;
		return false;
	}

	public void SetVertices( ReadOnlySpan<Vector3> span ) => Vertices = span.ToArray();
	public void SetVertices( IEnumerable<Vector3> items ) => Vertices = items is Vector3[] a ? a : items.ToArray();

	public void SetEdgeIndices( ReadOnlySpan<EdgeIndices> span ) => EdgeIndices = span.ToArray();
	public void SetEdgeIndices( IEnumerable<EdgeIndices> items ) => EdgeIndices = items is EdgeIndices[] a ? a : items.ToArray();

	public void SetSurfaceEdges( ReadOnlySpan<int> span ) => SurfaceEdges = span.ToArray();
	public void SetSurfaceEdges( IEnumerable<int> items ) => SurfaceEdges = items is int[] a ? a : items.ToArray();

	public void SetLeafFaceIndices( ReadOnlySpan<ushort> span ) => LeafFaceIndices = span.ToArray();
	public void SetLeafFaceIndices( IEnumerable<ushort> items ) => LeafFaceIndices = items is ushort[] a ? a : items.ToArray();

	public void SetFaces( ReadOnlySpan<Face> span ) => Faces = span.ToArray();
	public void SetFaces( IEnumerable<Face> items ) => Faces = items is Face[] a ? a : items.ToArray();

	public void SetOriginalFaces( ReadOnlySpan<Face> span ) => OriginalFaces = span.ToArray();
	public void SetOriginalFaces( IEnumerable<Face> items ) => OriginalFaces = items is Face[] a ? a : items.ToArray();

	public void SetDisplacementVertices( ReadOnlySpan<DisplacementVertex> span ) => DisplacementVertices = span.ToArray();
	public void SetDisplacementVertices( IEnumerable<DisplacementVertex> items ) => DisplacementVertices = items is DisplacementVertex[] a ? a : items.ToArray();

	public void SetDisplacementInfos( ReadOnlySpan<DisplacementInfo> span ) => DisplacementInfos = span.ToArray();
	public void SetDisplacementInfos( IEnumerable<DisplacementInfo> items ) => DisplacementInfos = items is DisplacementInfo[] a ? a : items.ToArray();
}
