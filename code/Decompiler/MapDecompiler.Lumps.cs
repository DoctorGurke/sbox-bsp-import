namespace BspImport.Decompiler;

public partial class MapDecompiler
{
	protected virtual BaseLump? ParseLump( int index, byte[] data, int version )
	{
		switch ( (LumpType)index )
		{
			case LumpType.Entity:
				return new EntityLump( Context, data );
			case LumpType.TexData:
				return new TexDataLump( Context, data );
			case LumpType.Vertex:
				return new VertexLump( Context, data );
			case LumpType.Node:
				return new NodeLump( Context, data );
			case LumpType.TexInfo:
				return new TexInfoLump( Context, data );
			case LumpType.Face:
				return new FaceLump( Context, data );
			case LumpType.Leaf:
				return new LeafLump( Context, data );
			case LumpType.Edge:
				return new EdgeLump( Context, data );
			case LumpType.SurfaceEdge:
				return new SurfaceEdgeLump( Context, data );
			case LumpType.Model:
				return new ModelLump( Context, data );
			case LumpType.LeafFace:
				return new LeafFaceLump( Context, data );
			case LumpType.DisplacementInfo:
				return new DisplacementInfoLump( Context, data );
			case LumpType.OriginalFace:
				return new OriginalFaceLump( Context, data );
			case LumpType.DisplacementVertices:
				return new DisplacementVertexLump( Context, data );
			case LumpType.Game:
				return new GameLumpHeader( Context, data );
			case LumpType.TexDataStringData:
				return new TexDataStringDataLump( Context, data );
			case LumpType.TexDataStringTable:
				return new TexDataStringTableLump( Context, data );
			default:
				throw new ArgumentException( $"Tried parsing Lump with unknown type! ({index})" );
		}
	}
}

public enum LumpType
{
	Entity = 0,
	TexData = 2,
	Vertex = 3,
	Node = 5,
	TexInfo = 6,
	Face = 7,
	Leaf = 10,
	Edge = 12,
	SurfaceEdge = 13,
	Model = 14,
	LeafFace = 16,
	DisplacementInfo = 26,
	OriginalFace = 27,
	DisplacementVertices = 33,
	Game = 35,
	TexDataStringData = 43,
	TexDataStringTable = 44,
}

