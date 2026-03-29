using BspImport.Decompiler.Formats.Readers;

namespace BspImport.Decompiler.Formats.Descriptors;

/// <summary>
/// Format descriptor for Source Engine BSP version 21 (Left 4 Dead 2 era).
///
/// V21 shares face and leaf binary layouts with v20; the primary differences are in
/// game lump static-prop struct versions and some new / repurposed lumps. Geometry
/// parsing is therefore identical to v20.
///
/// Games: Left 4 Dead, Left 4 Dead 2, Alien Swarm, etc.
/// </summary>
public sealed class SourceV21BspFormatDescriptor : IBspFormatDescriptor
{
	private static readonly IBspStructReaders _readers = new StandardBspStructReaders();

	private static readonly HashSet<string> L4DSignatures = new( StringComparer.OrdinalIgnoreCase )
	{
		"info_survivor_rescue",
		"info_director",
		"info_l4d_survivorset",
		"info_survivor_position",
	};

	private static readonly HashSet<string> AlienSwarmSignatures = new( StringComparer.OrdinalIgnoreCase )
	{
		"asw_marine_resource",
		"asw_weapon_spawner",
	};

	public BspGameFormat GameFormat => BspGameFormat.SourceV21;
	public int BspVersion => 21;
	public string DisplayName => "Source Engine BSP v21 (L4D / L4D2 / Alien Swarm / ...)";
	public bool IsDefinitiveForVersion => false;

	public bool MatchesEntities( IReadOnlyList<string> entityClassNames )
	{
		return L4DSignatures.Overlaps( entityClassNames )
		       || AlienSwarmSignatures.Overlaps( entityClassNames );
	}

	public IBspStructReaders StructReaders => _readers;
}
