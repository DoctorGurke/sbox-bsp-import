namespace BspImport.Decompiler;

public partial class MapDecompiler
{
	protected virtual BaseLump? ParseLump( LumpType lumpType, byte[] data, int version )
	{
		return lumpType switch
		{
			LumpType.Entity => new EntityLump( Context, data ),
			LumpType.Plane => new PlaneLump( Context, data ),
			LumpType.TexData => new TexDataLump( Context, data ),
			LumpType.Vertex => new VertexLump( Context, data ),
			LumpType.Node => new NodeLump( Context, data ),
			LumpType.TexInfo => new TexInfoLump( Context, data ),
			LumpType.Face => new FaceLump( Context, data ),
			LumpType.Leaf => new LeafLump( Context, data ),
			LumpType.Edge => new EdgeLump( Context, data ),
			LumpType.SurfaceEdge => new SurfaceEdgeLump( Context, data ),
			LumpType.Model => new ModelLump( Context, data ),
			LumpType.LeafFace => new LeafFaceLump( Context, data ),
			LumpType.Brush => new BrushLump( Context, data ),
			LumpType.DisplacementInfo => new DisplacementInfoLump( Context, data ),
			LumpType.OriginalFace => new OriginalFaceLump( Context, data ),
			LumpType.DisplacementVertices => new DisplacementVertexLump( Context, data ),
			LumpType.Game => new GameLumpHeader( Context, data ),
			LumpType.TexDataStringData => new TexDataStringDataLump( Context, data ),
			LumpType.TexDataStringTable => new TexDataStringTableLump( Context, data ),
			_ => throw new ArgumentException( $"Tried parsing Lump with unknown type!" )
		};
	}
}

public enum LumpType
{
	Entity = 0,
	Plane = 1,
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
	Brush = 18,
	DisplacementInfo = 26,
	OriginalFace = 27,
	DisplacementVertices = 33,
	Game = 35,
	TexDataStringData = 43,
	TexDataStringTable = 44,
}

