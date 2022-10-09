using BspImport.Decompiler.Lumps;

namespace BspImport.Decompiler;

public class DecompilerContext
{
	public IEnumerable<byte>? Data { get; set; }
	public BaseLump[] Lumps = new BaseLump[64];
	public IEnumerable<LumpEntity>? Entities { get; set; }
	public IEnumerable<MapModel>? Models { get; set; }
	public IEnumerable<GameLump>? GameLumps { get; set; }
}
