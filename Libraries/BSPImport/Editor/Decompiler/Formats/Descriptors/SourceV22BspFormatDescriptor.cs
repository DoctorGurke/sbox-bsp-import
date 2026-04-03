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

	public BspGameFormat GameFormat => BspGameFormat.SourceV22;
	public IReadOnlySet<int> SupportedVersions { get; } = new HashSet<int> { 22 };
	public string DisplayName => "Source Engine BSP v22";
	public int SpecificityScore => 50;

	public LumpHeaderLayout LumpHeaderLayout => LumpHeaderLayout.Standard;
	public BrushSideLayout BrushSideLayout => BrushSideLayout.Standard;
	public StaticPropLayout StaticPropLayout => StaticPropLayout.V10;

	public IBspStructReaders GetStructReaders( int bspVersion ) => _readers;
	public bool MatchesMapName( string mapName ) => false;
	public bool MatchesEntities( IReadOnlyList<string> entityClassNames ) => true;
}
