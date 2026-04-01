using BspImport.Decompiler.Formats.Readers;

namespace BspImport.Decompiler.Formats.Descriptors;

/// <summary>
/// Format descriptor for Source Engine BSP version 21 (Left 4 Dead 2 era).
///
/// Games: Left 4 Dead 2, Alien Swarm, Portal 2, etc.
/// </summary>
public sealed class SourceV21BspFormatDescriptor : IBspFormatDescriptor
{
	private static readonly IBspStructReaders _readers = new StandardBspStructReaders();

	public BspGameFormat GameFormat => BspGameFormat.SourceV21;
	public IReadOnlySet<int> SupportedVersions { get; } = new HashSet<int> { 20, 21 };
	public string DisplayName => "Source Engine BSP v21 (L4D2 / Alien Swarm / Portal 2)";
	public int SpecificityScore => 50;

	public LumpHeaderLayout LumpHeaderLayout => LumpHeaderLayout.Standard;
	public BrushSideLayout BrushSideLayout => BrushSideLayout.Standard;
	public StaticPropLayout StaticPropLayout => StaticPropLayout.V4;

	public IBspStructReaders GetStructReaders( int bspVersion ) => _readers;
	public bool MatchesMapName( string mapName ) => false;
	public bool MatchesEntities( IReadOnlyList<string> entityClassNames ) => true;
}
