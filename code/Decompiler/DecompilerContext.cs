using BspImport.Decompiler.Lumps;

namespace BspImport.Decompiler;

public static class DecompilerContext
{
	public static IEnumerable<byte>? Data { get; set; }
	public static BaseLump[] Lumps = new BaseLump[64];
	public static IEnumerable<LumpEntity>? Entities { get; set; }
	public static IEnumerable<MapModel>? Models { get; set; }
	public static IEnumerable<GameLump>? GameLumps { get; set; }
}
