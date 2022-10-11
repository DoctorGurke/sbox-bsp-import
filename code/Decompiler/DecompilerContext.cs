using BspImport.Decompiler.Lumps;
using Tools.MapDoc;

namespace BspImport.Decompiler;

public class DecompilerContext
{
	public DecompilerContext()
	{
		Lumps = new BaseLump[64];
		MapGeometry = new();
	}

	public bool Decompiled { get; set; } = false;
	public bool Decompiling { get; set; } = false;
	public bool PreparedWorldSpawn { get; set; } = false;

	public IEnumerable<byte>? Data { get; set; }
	public BaseLump[] Lumps;
	public IEnumerable<LumpEntity>? Entities { get; set; }
	public IEnumerable<MapModel>? Models { get; set; }
	public IEnumerable<GameLump>? GameLumps { get; set; }
	public MapGeometry MapGeometry { get; private set; }
	public IEnumerable<TexInfo>? TexInfo { get; set; }
	public IEnumerable<TexData>? TexData { get; set; }
	public IEnumerable<int>? TexDataStringTable { get; set; }
	public TexDataStringData TexDataStringData { get; set; }

	public PolygonMesh? WorldSpawn { get; set; }
}
