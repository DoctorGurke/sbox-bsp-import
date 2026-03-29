namespace BspImport.Decompiler.Formats;

/// <summary>
/// Identifies the specific BSP game format variant being parsed.
/// Values intentionally match the BSP version number for the unique-version cases.
/// </summary>
public enum BspGameFormat
{
	Unknown = 0,

	/// <summary>
	/// Vampire: The Masquerade - Bloodlines (BSP v17).
	/// Substantially modified dface_t (104 bytes): prepends 8× colorRGBExp32 avg-light
	/// colours, and uses MAXLIGHTMAPS=8 with extra day/night lighting arrays.
	/// dleaf_t is 32 bytes (ambient lighting omitted from leaf, same as v20+).
	/// </summary>
	VampireBloodlines = 17,

	/// <summary>
	/// Standard Source Engine BSP version 20.
	/// Adds HDR lighting lumps (53–56). Ambient lighting moved out of dleaf_t (now 32 bytes).
	/// Standard dface_t is 56 bytes.
	/// Games: HL2 (HDR update), HL2:EP1/EP2, CS:S, DoD:S, TF2, Portal, etc.
	/// </summary>
	SourceV20 = 20,

	/// <summary>
	/// Source Engine BSP version 21 (Left 4 Dead 2 era).
	/// Shares face and leaf binary layout with v20.
	/// Primary differences are in game lump static-prop struct versions.
	/// Games: L4D, L4D2, Alien Swarm, etc.
	/// </summary>
	SourceV21 = 21,

	/// <summary>
	/// Source Engine BSP version 22 (Portal 2 / CS:GO era).
	/// Shares face and leaf binary layout with v20/v21.
	/// Games: CS:GO, Portal 2, DOTA 2, etc.
	/// </summary>
	SourceV22 = 22,
}
