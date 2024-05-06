using BspImport.Builder;

namespace BspImport;

public class ImportContext
{
	public ImportContext( byte[] data )
	{
		Data = data;

		Lumps = new BaseLump[64];
		Geometry = new();
		CachedMaterials = new();
	}

	/// <summary>
	/// Decompiles the data of the context.
	/// </summary>
	public void Decompile()
	{
		var decompiler = new MapDecompiler( this );
		decompiler.Decompile();
	}

	/// <summary>
	/// Construct the decompiled context into the scene.
	/// </summary>
	public void Build()
	{
		var builder = new MapBuilder( this );
		builder.Build();
	}

	public byte[] Data { get; private set; }
	public BaseLump[] Lumps;

	// bsp tree structure
	public Decompiler.Lumps.MapNode[]? Nodes;
	public MapLeaf[]? Leafs;

	public LumpEntity[]? Entities { get; set; }
	public MapModel[]? Models { get; set; }
	public GameLump[]? GameLumps { get; set; }
	public MapGeometry Geometry { get; private set; }
	public TexInfo[]? TexInfo { get; set; }
	public TexData[]? TexData { get; set; }
	public int[]? TexDataStringTable { get; set; }
	public TexDataStringData TexDataStringData { get; set; }

	public PolygonMesh? WorldSpawn { get; set; }
	public PolygonMesh[]? CachedPolygonMeshes { get; set; }
	public Dictionary<string, Material> CachedMaterials { get; set; }
}
