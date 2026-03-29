using BspImport.Decompiler.Formats.Descriptors;

namespace BspImport.Decompiler.Formats;

/// <summary>
/// Registry of all known BSP format descriptors.
/// Drives two-phase format detection: version-first, then optional entity refinement.
///
/// <b>Adding a new game format:</b>
/// Implement <see cref="IBspFormatDescriptor"/>, add an instance to
/// <see cref="Descriptors"/> in the correct position. No other changes required.
/// </summary>
public static class BspFormatRegistry
{
	/// <summary>
	/// All registered format descriptors, ordered from most-specific to least-specific.
	/// Add new descriptors here to extend support.
	/// </summary>
	private static readonly IReadOnlyList<IBspFormatDescriptor> Descriptors =
		new List<IBspFormatDescriptor>
		{
            // definitively versioned entries first.
            new VtmbBspFormatDescriptor(),

            // shared version entries, ordered by entity-match specificity.
            new SourceV22BspFormatDescriptor(),
			new SourceV21BspFormatDescriptor(),
			new SourceV20BspFormatDescriptor() // broadest match, always last for v20.
        };

	/// <summary>
	/// Phase 1: detect the format descriptor from the file's BSP version number alone.
	/// For definitively-versioned formats the result is final.
	/// For shared-version formats, call <see cref="RefineWithEntities"/> afterward.
	/// </summary>
	public static IBspFormatDescriptor DetectByVersion( int version )
	{
		// definitive match: only one descriptor owns this version.
		var definitive = Descriptors.FirstOrDefault(
			d => d.BspVersion == version && d.IsDefinitiveForVersion );

		if ( definitive is not null )
		{
			Log.Info( $"[BSP] Version {version}: '{definitive.DisplayName}' (definitive)." );
			return definitive;
		}

		// fall back to first non-definitive match (broadest match / default).
		var fallback = Descriptors.FirstOrDefault( d => d.BspVersion == version );

		if ( fallback is not null )
		{
			Log.Info( $"[BSP] Version {version} matched '{fallback.DisplayName}'. " +
					  "Entity refinement available after lump 0 is parsed." );
			return fallback;
		}

		Log.Warning( $"[BSP] Unrecognised BSP version {version}. " +
					 "Falling back to unknown-format descriptor (standard v20 readers)." );
		return UnknownBspFormatDescriptor.Instance;
	}

	/// <summary>
	/// Phase 2: refine an already-detected descriptor using entity classnames from lump 0.
	/// Returns the same descriptor unchanged if <see cref="IBspFormatDescriptor.IsDefinitiveForVersion"/>
	/// is true, or if no better candidate is found.
	/// </summary>
	/// <param name="current">Descriptor returned by Phase 1.</param>
	/// <param name="entityClassNames">
	/// Distinct classnames of all entities in lump 0.
	/// Callers typically produce this with:
	/// <c>context.Entities?.Select(e => e.ClassName).Distinct().ToList()</c>
	/// </param>
	public static IBspFormatDescriptor RefineWithEntities(
		IBspFormatDescriptor current,
		IReadOnlyList<string> entityClassNames )
	{
		if ( current.IsDefinitiveForVersion )
			return current;

		var candidates = Descriptors
			.Where( d => d.BspVersion == current.BspVersion )
			.ToList();

		var refined = candidates.FirstOrDefault( d => d.MatchesEntities( entityClassNames ) );

		if ( refined is null || refined.GameFormat == current.GameFormat )
		{
			return current;
		}

		Log.Info( $"[BSP] Entity refinement: '{current.DisplayName}' → '{refined.DisplayName}'." );
		return refined;
	}
}
