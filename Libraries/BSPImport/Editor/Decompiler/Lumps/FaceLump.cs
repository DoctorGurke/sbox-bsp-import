using BspImport.Builder;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using System.Globalization;
using static Editor.MeshEditor.PrimitiveBuilder.PolygonMesh;
using static Sandbox.ParticleSnapshot;

namespace BspImport.Decompiler.Lumps;

public class FaceLump : BaseLump
{
	public FaceLump( ImportContext context, byte[] data, int version = 0 ) : base( context, data, version ) { }

	protected override void Parse( BinaryReader reader )
	{
		var structReaders = Context.FormatDescriptor.GetStructReaders( Context.BspVersion );

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
	public short SurfaceFogVolumeIndex;
	public float Area;
	public int OriginalFaceIndex;

	public Face( int firstEdge, short edgeCount, short texInfo, short dispInfo, short surfaceFogVolumeID, float area, int oFace )
	{
		FirstEdge = firstEdge;
		EdgeCount = edgeCount;
		TexInfo = texInfo;
		DisplacementInfo = dispInfo;
		SurfaceFogVolumeIndex = surfaceFogVolumeID;
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
		return context.TexDataStringData?.FromStringTableIndex( stringDataIndex ).ToLower() ?? null;
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

	public SurfaceFlags GetSurfaceFlags( ImportContext context )
	{
		var texInfo = context.TexInfo?[TexInfo];

		if ( texInfo is null )
			return 0;

		return texInfo.Value.Flags;
	}

	public List<Vector3>? GetEdgeVertices( ImportContext context )
	{
		if ( !context.HasCompleteGeometry( out var geo ) )
			return null;

		var verts = new List<Vector3>();

		// get verts from surf edges -> edges -> vertices
		for ( int i = 0; i < EdgeCount; i++ )
		{
			int surfEdgeIdx = FirstEdge + i;
			if ( !geo.TryGetSurfaceEdge( surfEdgeIdx, out var edge ) )
				return null;

			// edge sign affects winding order, indexing back to front or vice versa on the edge vertices
			int edgeIndex = edge >= 0 ? edge : -edge;
			if ( !geo.TryGetEdgeIndices( edgeIndex, out var edgeIndices ) )
				return null;

			var indices = edgeIndices.Indices;
			if ( indices is null || indices.Length < 2 )
				return null;

			int vertIdx = edge >= 0 ? indices[0] : indices[1];
			if ( !geo.TryGetVertex( vertIdx, out var vertex ) )
				return null;

			verts.Add( vertex );
		}

		return verts;
	}
}
