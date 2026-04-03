using BspImport.Decompiler.Formats.Readers;

namespace BspImport.Decompiler.Formats.Descriptors;

public sealed class CounterStrikeGlobalOffensiveBspFormatDescriptor : IBspFormatDescriptor
{
	private static readonly IBspStructReaders _readers = new StandardBspStructReaders();

	private static readonly HashSet<string> Signatures = new( StringComparer.OrdinalIgnoreCase )
	{
		"cs_gamerules",
		"dangerzone_controller",
		"flashbang_projectile",
		"func_bomb_target",
		"func_buyzone",
		"func_hostage_rescue",
		"func_no_defuse",
		"hostage_entity",
		"info_deathmatch_spawn",
		"info_hostage_spawn",
		"info_player_counterterrorist",
		"info_player_terrorist",
		"planted_c4_training"
	};

	public BspGameFormat GameFormat => BspGameFormat.CounterStrikeGlobalOffensive;
	public IReadOnlySet<int> SupportedVersions { get; } = new HashSet<int> { 21 };
	public string DisplayName => "Counter-Strike: Global Offensive BSP";
	public int SpecificityScore => 100;

	public LumpHeaderLayout LumpHeaderLayout => LumpHeaderLayout.Standard;
	public BrushSideLayout BrushSideLayout => BrushSideLayout.Standard;
	public StaticPropLayout StaticPropLayout => StaticPropLayout.V11;

	public IBspStructReaders GetStructReaders( int bspVersion ) => _readers;

	public bool MatchesMapName( string mapName ) => mapName.StartsWith( "de_", StringComparison.OrdinalIgnoreCase )
	                                                || mapName.StartsWith( "cs_", StringComparison.OrdinalIgnoreCase )
	                                                || mapName.StartsWith( "aim_", StringComparison.OrdinalIgnoreCase )
	                                                || mapName.StartsWith( "ar_", StringComparison.OrdinalIgnoreCase );

	public bool MatchesEntities( IReadOnlyList<string> entityClassNames )
	{
		return Signatures.Overlaps( entityClassNames );
	}
}
