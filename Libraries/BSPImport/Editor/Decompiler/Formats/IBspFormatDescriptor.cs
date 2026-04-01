namespace BspImport.Decompiler.Formats;

/// <summary>
/// Describes a specific BSP format variant: how to detect it and how to read its structures.
/// Implement this interface to add support for a new BSP game format without modifying
/// existing code. Just register the new descriptor in <see cref="BspFormatRegistry"/>.
/// </summary>
public interface IBspFormatDescriptor
{
	/// <summary>The game format enum value this descriptor represents.</summary>
	BspGameFormat GameFormat { get; }

	/// <summary>
	/// All BSP version numbers this format handles.
	/// A descriptor may support more than one version when a game shipped maps in
	/// multiple format revisions (e.g. HL2 beta shipped v17 and v18 maps).
	/// </summary>
	IReadOnlySet<int> SupportedVersions { get; }

	/// <summary> Human-readable name used in log messages and the import context. </summary>
	string DisplayName { get; }

	/// <summary>
	/// Specificity score used to order competing candidates that share a version number.
	/// The registry tries higher-scoring descriptors before lower-scoring ones.
	/// A descriptor whose <see cref="MatchesEntities"/> always returns <c>true</c>
	/// (broadest fallback) should have score 0.
	///
	/// Suggested scale:
	/// <list type="bullet">
	///   <item>100 — single-game format with unique version (e.g. VTMB v17).</item>
	///   <item>50  — multi-version format or game with distinct entity signatures.</item>
	///   <item>10  — game with weak / shared entity signatures.</item>
	///   <item>0   — broadest catch-all fallback for a version group.</item>
	/// </list>
	/// </summary>
	int SpecificityScore { get; }

	LumpHeaderLayout LumpHeaderLayout { get; }
	BrushSideLayout BrushSideLayout { get; }
	StaticPropLayout StaticPropLayout { get; }

	/// <summary>
	/// Returns the binary struct readers appropriate for the given BSP version number.
	/// Allows a single descriptor to handle multiple versions that have slightly
	/// different layouts (e.g. a hypothetical descriptor for HL2 beta could return
	/// different readers for v17 vs. v18).
	/// </summary>
	/// <param name="bspVersion">The exact version read from the BSP file header.</param>
	IBspStructReaders GetStructReaders( int bspVersion );

	/// <summary>
	/// Fast pre-filter using the map file name or path.
	/// Called before <see cref="MatchesEntities"/> as it requires no lump parsing.
	///
	/// Return <c>true</c> if the map name is consistent with this format (or if this
	/// descriptor has no opinion on naming). Return <c>false</c> only when the name
	/// is a reliable negative signal (e.g. a VTMB-prefix map can't be HL2 beta).
	///
	/// The default implementation should return <c>true</c> (no-op filter).
	/// </summary>
	/// <param name="mapName">
	/// The bare filename without extension, e.g. <c>"la_sewers"</c> or <c>"d1_trainstation_01"</c>.
	/// </param>
	bool MatchesMapName( string mapName );

	/// <summary>
	/// Secondary identification using entity classnames from lump 0.
	/// Only called when multiple descriptors share the same version number.
	/// Return <c>true</c> if the provided entity composition is consistent with this format.
	/// </summary>
	/// <param name="entityClassNames">
	/// Distinct classnames of all entities parsed from lump 0.
	/// </param>
	bool MatchesEntities( IReadOnlyList<string> entityClassNames );
}
