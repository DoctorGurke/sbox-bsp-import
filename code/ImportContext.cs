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

		var decompiler = new MapDecompiler( this );
		decompiler.Decompile();
	}

	/// <summary>
	/// Construct the decompiled context in the active map.
	/// </summary>
	public void Build()
	{
		var builder = new MapBuilder( this );
		builder.CacheMaterials();
		builder.CachePolygonMeshes();
		builder.Build();
	}

	public byte[] Data { get; private set; }
	public BaseLump[] Lumps;
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
