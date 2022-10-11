using BspImport.Decompiler.Lumps;
using System.Threading.Tasks;
using Tools.MapDoc;

namespace BspImport.Decompiler;

public class DecompilerContext
{
	public DecompilerContext( byte[] data )
	{
		Data = data;

		Lumps = new BaseLump[64];
		MapGeometry = new();
		CachedMaterials = new();
	}

	public Task? DecompileTask { get; set; }
	public Task? CacheTask { get; set; }

	public byte[] Data { get; private set; }
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
