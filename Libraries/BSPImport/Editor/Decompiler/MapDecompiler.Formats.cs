
using BspImport.Decompiler.Formats;

namespace BspImport.Decompiler;

public partial class MapDecompiler
{
	/// <summary>
	/// Refine format using the map file name (no lump parsing needed).
	/// Called in Decompile() immediately after initial phase, using Context.Name.
	/// </summary>
	private void RefineFormatWithMapName( int bspVersion )
	{
		var mapName = Path.GetFileNameWithoutExtension( Context.Name );
		Context.FormatDescriptor = BspFormatRegistry.RefineWithMapName(
			Context.FormatDescriptor, bspVersion, mapName );
	}

	/// <summary>
	/// Refines the BSP format using parsed entity classnames.
	/// Helps disambiguate shared BSP versions; otherwise no-op.
	/// Returns immediately if no entities are present.
	/// </summary>
	private void RefineFormatFromEntities( int bspVersion )
	{
		if ( Context.Entities is not { Length: > 0 } )
			return;

		var classNames = Context.Entities
			.Select( e => e.ClassName )
			.Where( c => !string.IsNullOrEmpty( c ) )
			.Select( c => c! )
			.Distinct( StringComparer.OrdinalIgnoreCase )
			.ToList();

		Context.FormatDescriptor = BspFormatRegistry.RefineWithEntities(
			Context.FormatDescriptor, bspVersion, classNames );
	}
}
