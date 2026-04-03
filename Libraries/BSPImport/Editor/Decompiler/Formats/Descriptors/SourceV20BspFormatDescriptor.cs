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

	public BspGameFormat GameFormat => BspGameFormat.SourceV20;
	public IReadOnlySet<int> SupportedVersions { get; } = new HashSet<int> { 19, 20 };
	public string DisplayName => "Source Engine BSP v20 (HL2 / CS:S / DoD:S / TF2 / Portal / ...)";
	/// <summary>
	/// Score 0 — this is the broadest catch-all. Any more-specific v19/v20 descriptor
	/// should use a higher score.
	/// </summary>
	public int SpecificityScore => 0;

	public LumpHeaderLayout LumpHeaderLayout => LumpHeaderLayout.Standard;

	public BrushSideLayout BrushSideLayout => BrushSideLayout.Standard;

	public StaticPropLayout StaticPropLayout => StaticPropLayout.V4;

	/// <summary>
	/// All v19/v20 variants share the standard 56-byte face layout.
	/// The bspVersion parameter is ignored — same readers for both.
	/// </summary>
	public IBspStructReaders GetStructReaders( int bspVersion ) => _readers;

	/// <summary>No reliable naming opinion, so do not participate in map-name refinement.</summary>
	public bool MatchesMapName( string mapName ) => false;

	/// <summary>
	/// Always returns true. This descriptor is the broadest fallback for v20 files.
	/// Register more-specific v20 sub-format descriptors (e.g. per-game) before this
	/// one in the registry to allow finer disambiguation.
	/// </summary>
	public bool MatchesEntities( IReadOnlyList<string> entityClassNames ) => true;
}
