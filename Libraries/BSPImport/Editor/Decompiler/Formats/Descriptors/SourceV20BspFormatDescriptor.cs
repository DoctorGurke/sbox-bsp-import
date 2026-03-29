using BspImport.Decompiler.Formats.Readers;

namespace BspImport.Decompiler.Formats.Descriptors;

/// <summary>
/// Format descriptor for Source Engine BSP version 20.
///
/// V20 is shared across many Source games so <see cref="IsDefinitiveForVersion"/> is false.
/// Entity-based refinement is available for distinguishing between games when needed.
/// This descriptor acts as the broadest / fallback match for v20 files; it should be
/// registered last in <see cref="BspFormatRegistry"/> for v20.
///
/// Games: HL2 (HDR update), HL2:EP1, HL2:EP2, CS:S, DoD:S, TF2, Portal, and others.
/// </summary>
public sealed class SourceV20BspFormatDescriptor : IBspFormatDescriptor
{
	private static readonly IBspStructReaders _readers = new StandardBspStructReaders();

	// Known entity classnames exclusive to specific v20 games.
	// Used for optional disambiguation if callers request entity refinement.
	private static readonly HashSet<string> TF2Signatures = new( StringComparer.OrdinalIgnoreCase )
		{ "tf_gamerules", "item_teamflag", "func_respawnroom", "info_observer_point" };

	private static readonly HashSet<string> CSSSignatures = new( StringComparer.OrdinalIgnoreCase )
		{ "cs_team_manager", "func_bomb_target", "cs_player_manager", "hostage_entity" };

	private static readonly HashSet<string> DodSSignatures = new( StringComparer.OrdinalIgnoreCase )
		{ "dod_control_point", "dod_round_master", "dod_team_manager" };

	public BspGameFormat GameFormat => BspGameFormat.SourceV20;
	public int BspVersion => 20;
	public string DisplayName => "Source Engine BSP v20 (HL2 / CS:S / DoD:S / TF2 / Portal / ...)";
	public bool IsDefinitiveForVersion => false;

	/// <summary>
	/// Always returns true. This descriptor is the broadest fallback for v20 files.
	/// Register more-specific v20 sub-format descriptors (e.g. per-game) before this
	/// one in the registry to allow finer disambiguation.
	/// </summary>
	public bool MatchesEntities( IReadOnlyList<string> entityClassNames ) => true;

	public IBspStructReaders StructReaders => _readers;
}
