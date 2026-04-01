using BspImport.Decompiler.Formats.Readers;

namespace BspImport.Decompiler.Formats.Descriptors;

/// <summary>
/// Format descriptor for Vampire: The Masquerade - Bloodlines (BSP version 17).
/// </summary>
public sealed class VtmbBspFormatDescriptor : IBspFormatDescriptor
{
	private static readonly IBspStructReaders _readers = new VtmbBspStructReaders();

	/// <summary>
	/// Known VTMB map name prefixes. All retail VTMB maps follow this convention.
	/// </summary>
	private static readonly string[] MapPrefixes = ["la_", "sm_", "ch_", "hw_"];

	private static readonly HashSet<string> Signatures = new( StringComparer.OrdinalIgnoreCase )
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
	public IReadOnlySet<int> SupportedVersions { get; } = new HashSet<int> { 17 };
	public string DisplayName => "Vampire: The Masquerade – Bloodlines (BSP v17)";
	public int SpecificityScore => 100;

	public LumpHeaderLayout LumpHeaderLayout => LumpHeaderLayout.Standard;
	public BrushSideLayout BrushSideLayout => BrushSideLayout.Standard;
	public StaticPropLayout StaticPropLayout => StaticPropLayout.V4;

	public IBspStructReaders GetStructReaders( int bspVersion ) => _readers;

	public bool MatchesMapName( string mapName )
	{
		if ( string.IsNullOrEmpty( mapName ) )
			return true;

		return MapPrefixes.Any( prefix =>
			mapName.StartsWith( prefix, StringComparison.OrdinalIgnoreCase ) );
	}

	public bool MatchesEntities( IReadOnlyList<string> entityClassNames )
	{
		return Signatures.Overlaps( entityClassNames );
	}
}
