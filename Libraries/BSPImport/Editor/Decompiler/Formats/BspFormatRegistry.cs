using BspImport.Decompiler.Formats.Descriptors;

namespace BspImport.Decompiler.Formats;

/// <summary>
/// Registry of all known BSP format descriptors.
/// Drives two-phase format detection: version-first, then optional entity refinement.
///
/// <b>Adding a new game format:</b>
/// Implement <see cref="IBspFormatDescriptor"/>, add an instance to
/// <see cref="GetDescriptors"/> in the correct position. No other changes required.
/// </summary>
public static class BspFormatRegistry
{
	/// <summary>
	/// Phase 1: detect the format descriptor from the file's BSP version number alone.
	/// For definitively-versioned formats the result is final.
	/// For shared-version formats, call <see cref="RefineWithEntities"/> afterward.
	/// </summary>
	public static IBspFormatDescriptor DetectByVersion( int version )
	{
		var candidates = CandidatesForVersion( version );

		if ( candidates.Count == 0 )
		{
			Log.Warning( $"[BSP] Unrecognised version {version}. " +
			             "Using unknown-format fallback (standard Source v20 readers)." );
			return UnknownBspFormatDescriptor.Instance;
		}

		if ( IsVersionDefinitive( version ) )
		{
			var winner = candidates[0];
			Log.Info( $"[BSP] v{version} → '{winner.DisplayName}' (definitive — sole owner)." );
			return winner;
		}

		// NEW: always start with the generic fallback (lowest SpecificityScore)
		// Because candidates are already ordered descending by score, the last one is the safest.
		var fallback = candidates[^1];

		Log.Info( $"[BSP] v{version} → '{fallback.DisplayName}' " +
		          $"(fallback — {candidates.Count} formats share this version; " +
		          "map name and/or entity refinement pending)." );

		return fallback;
	}

	/// <summary>
	/// Refine using the map file name (no lump parsing required).
	/// Safe to call immediately after first phase. No-op for definitively-versioned formats.
	/// </summary>
	/// <param name="current">Descriptor from first phase.</param>
	/// <param name="bspVersion">The exact BSP version from the file header.</param>
	/// <param name="mapName">
	/// Bare map filename without extension, e.g. <c>"la_sewers"</c>.
	/// Typically <c>Path.GetFileNameWithoutExtension(Context.Name)</c>.
	/// </param>
	public static IBspFormatDescriptor RefineWithMapName( IBspFormatDescriptor current,
		int bspVersion,
		string mapName )
	{
		if ( IsVersionDefinitive( bspVersion ) || string.IsNullOrEmpty( mapName ) )
			return current;

		var refined = SelectBestMatchingCandidate( bspVersion,
			d => d.MatchesMapName( mapName ) );

		return LogRefinement( current, refined, $"map name '{mapName}'" );
	}

	/// <summary>
	/// Refine using entity classnames (no lump parsing required).
	/// </summary>
	/// <param name="current"></param>
	/// <param name="bspVersion"></param>
	/// <param name="entityClassNames"></param>
	/// <returns></returns>
	public static IBspFormatDescriptor RefineWithEntities(
		IBspFormatDescriptor current,
		int bspVersion,
		IReadOnlyList<string> entityClassNames )
	{
		if ( IsVersionDefinitive( bspVersion ) || entityClassNames is not { Count: > 0 } )
			return current;

		var refined = SelectBestMatchingCandidate(
			bspVersion,
			d => d.MatchesEntities( entityClassNames ) );

		return LogRefinement( current, refined, "entity classnames" );
	}

	public static bool IsVersionDefinitive( int version ) =>
		GetVersionOwnerCount().TryGetValue( version, out var count ) && count == 1;

	private static IReadOnlyList<IBspFormatDescriptor> CandidatesForVersion( int version ) =>
		GetDescriptors()
			.Where( d => d.SupportedVersions.Contains( version ) )
			.OrderByDescending( d => d.SpecificityScore )
			.ThenBy( d => d.DisplayName, StringComparer.Ordinal )
			.ToList();

	/// <summary>
	/// Rebuild descriptors on demand instead of caching static instances so hot reload
	/// picks up registry edits without requiring a full editor/domain restart.
	/// </summary>
	private static IReadOnlyList<IBspFormatDescriptor> GetDescriptors()
	{
		return
		[
			new VtmbBspFormatDescriptor(),
			new Portal2BspFormatDescriptor(),
			new AlienSwarmBspFormatDescriptor(),
			new CounterStrikeGlobalOffensiveBspFormatDescriptor(),
			new Left4DeadBspFormatDescriptor(),
			new Left4Dead2BspFormatDescriptor(),
			new SourceV22BspFormatDescriptor(),
			new SourceV21BspFormatDescriptor(),
			new SourceV20BspFormatDescriptor()
		];
	}

	private static IReadOnlyDictionary<int, int> GetVersionOwnerCount()
	{
		return GetDescriptors()
			.SelectMany( d => d.SupportedVersions.Select( v => (version: v, descriptor: d) ) )
			.GroupBy( x => x.version )
			.ToDictionary( g => g.Key, g => g.Count() );
	}

	private static IBspFormatDescriptor? SelectBestMatchingCandidate(
		int bspVersion,
		Func<IBspFormatDescriptor, bool> predicate )
	{
		return CandidatesForVersion( bspVersion )
			.Where( predicate )
			.FirstOrDefault();
	}

	private static IBspFormatDescriptor LogRefinement(
		IBspFormatDescriptor current,
		IBspFormatDescriptor? refined,
		string refinementSource )
	{
		if ( refined is null || refined.GetType() == current.GetType() )
			return current;

		Log.Info( $"[BSP] Refined via {refinementSource}: " +
		          $"'{current.DisplayName}' → '{refined.DisplayName}'." );
		return refined;
	}
}
