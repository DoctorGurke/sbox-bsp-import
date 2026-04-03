using BspImport.Decompiler.Formats.Readers;

namespace BspImport.Decompiler.Formats.Descriptors;

/// <summary>
/// Fallback descriptor for BSP versions that have no registered handler.
/// Uses standard Source v20 struct readers as a best-effort fallback.
/// Log a warning when this is active so the caller can investigate.
/// </summary>
public sealed class UnknownBspFormatDescriptor : IBspFormatDescriptor
{
	/// <summary>Singleton instance, this descriptor holds no mutable state.</summary>
	public static readonly UnknownBspFormatDescriptor Instance = new();

	private static readonly IBspStructReaders _readers = new StandardBspStructReaders();

	private UnknownBspFormatDescriptor() { }

	public BspGameFormat GameFormat => BspGameFormat.Unknown;
	public IReadOnlySet<int> SupportedVersions { get; } = new HashSet<int>();
	public string DisplayName => "Unknown BSP format (fallback: Source v20 readers)";
	public int SpecificityScore => -1;

	public LumpHeaderLayout LumpHeaderLayout => LumpHeaderLayout.Standard;
	public BrushSideLayout BrushSideLayout => BrushSideLayout.Standard;
	public StaticPropLayout StaticPropLayout => StaticPropLayout.V4;

	public IBspStructReaders GetStructReaders( int bspVersion ) => _readers;
	public bool MatchesMapName( string mapName ) => false;
	public bool MatchesEntities( IReadOnlyList<string> entityClassNames ) => false;
}
