using BspImport.Decompiler.Formats.Readers;

namespace BspImport.Decompiler.Formats.Descriptors;

/// <summary>
/// Format descriptor for Source Engine BSP version 22 (Portal 2 / CS:GO era).
///
/// V22 shares face and leaf binary layouts with v20/v21, differences are again
/// confined to game lump static-prop struct versions and some newer lumps.
/// Geometry parsing is identical to v20.
///
/// Games: CS:GO, Portal 2, DOTA 2, and others.
/// </summary>
public sealed class SourceV22BspFormatDescriptor : IBspFormatDescriptor
{
	private static readonly IBspStructReaders _readers = new StandardBspStructReaders();

	private static readonly HashSet<string> CSGOSignatures = new( StringComparer.OrdinalIgnoreCase )
	{
		"cs_gamerules",
		"func_bomb_target",
		"cs_team_manager",
	};

	private static readonly HashSet<string> Portal2Signatures = new( StringComparer.OrdinalIgnoreCase )
	{
		"prop_laser_catcher",
		"info_coop_spawn",
		"prop_button",
		"info_fizzler",
	};

	private static readonly HashSet<string> Dota2Signatures = new( StringComparer.OrdinalIgnoreCase )
	{
		"ent_dota_game_events",
		"dota_item_spawner",
	};

	public BspGameFormat GameFormat => BspGameFormat.SourceV22;
	public int BspVersion => 22;
	public string DisplayName => "Source Engine BSP v22 (CS:GO / Portal 2 / DOTA 2 / ...)";
	public bool IsDefinitiveForVersion => false;

	public bool MatchesEntities( IReadOnlyList<string> entityClassNames )
	{
		return CSGOSignatures.Overlaps( entityClassNames )
		       || Portal2Signatures.Overlaps( entityClassNames )
		       || Dota2Signatures.Overlaps( entityClassNames );
	}

	public IBspStructReaders StructReaders => _readers;
}
