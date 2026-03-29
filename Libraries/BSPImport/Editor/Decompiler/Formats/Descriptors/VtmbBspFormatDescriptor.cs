using BspImport.Decompiler.Formats.Readers;

namespace BspImport.Decompiler.Formats.Descriptors;

/// <summary>
/// Format descriptor for Vampire: The Masquerade - Bloodlines (BSP version 17).
/// </summary>
public sealed class VtmbBspFormatDescriptor : IBspFormatDescriptor
{
	private static readonly IBspStructReaders _readers = new VtmbBspStructReaders();

	private static readonly HashSet<string> VTMBSignatures = new( StringComparer.OrdinalIgnoreCase )
	{
		"events_world",
		"events_player",
		"inspection_node",
		"intersting_place",
		"item_container_animated",
		"item_container_lock",
		"item_g_watch_fancy",
		"item_g_astrolite",
		"item_g_lockpick",
		"item_m_money_envelope",
		"inspection_node",
		"npc_VDialogPedestrian",
		"npc_VHumanCombatant",
		"npc_VPedestrian",
		"npc_VProneDialog",
		"npc_VVampire",
		"params_particle",
		"prop_doorknob",
		"prop_doorknob_electronic",
		"prop_hacking",
	};

	public BspGameFormat GameFormat => BspGameFormat.VampireBloodlines;
	public int BspVersion => 17;
	public string DisplayName => "Vampire: The Masquerade – Bloodlines (BSP v17)";

	/// <summary>
	/// BSP v17 is used by VTMB but also by some older Source engine versions (e.g. HL2 beta),
	/// so the version number alone is not definitive.
	/// </summary>
	public bool IsDefinitiveForVersion => false;

	/// <summary>
	/// Not called in practice because <see cref="IsDefinitiveForVersion"/> is true,
	/// but returns true as a safe default.
	/// </summary>
	public bool MatchesEntities( IReadOnlyList<string> entityClassNames )
	{
		return VTMBSignatures.Overlaps( entityClassNames );
	}

	public IBspStructReaders StructReaders => _readers;
}
