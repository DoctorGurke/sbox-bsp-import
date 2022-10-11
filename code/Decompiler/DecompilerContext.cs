using BspImport.Decompiler.Lumps;
using Tools.MapDoc;

namespace BspImport.Decompiler;

public class DecompilerContext
{
	public DecompilerContext()
	{
		Lumps = new BaseLump[64];
		MapGeometry = new();
		CachedMaterials = new();
	}

	public bool Decompiled { get; set; } = false;
	public bool Cached { get; set; } = false;

	public byte[]? Data { get; set; }
	public BaseLump[] Lumps;
	public LumpEntity[]? Entities { get; set; }
	public MapModel[]? Models { get; set; }
	public GameLump[]? GameLumps { get; set; }
	public MapGeometry MapGeometry { get; private set; }
	public TexInfo[]? TexInfo { get; set; }
	public TexData[]? TexData { get; set; }
	public int[]? TexDataStringTable { get; set; }
	public TexDataStringData TexDataStringData { get; set; }

	public PolygonMesh? WorldSpawn { get; set; }
	public PolygonMesh[]? CachedPolygonMeshes { get; set; }
	public Dictionary<string, Material> CachedMaterials { get; set; }
}
