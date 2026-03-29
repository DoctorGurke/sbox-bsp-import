
using BspImport.Decompiler.Formats;

namespace BspImport.Decompiler;

public partial class MapDecompiler
{
	/// <summary>
	/// Refines the BSP format using parsed entity classnames.
	/// Helps disambiguate shared BSP versions; otherwise no-op.
	/// Returns immediately if no entities are present.
	/// </summary>
	private void RefineFormatFromEntities()
	{
		if ( Context.Entities is not { Length: > 0 } )
			return;

		var classNames = Context.Entities
			.Select( e => e.ClassName )
			.Where( c => !string.IsNullOrEmpty( c ) )
			.Distinct( StringComparer.OrdinalIgnoreCase )
			.ToList();

		Context.FormatDescriptor = BspFormatRegistry.RefineWithEntities( Context.FormatDescriptor, classNames );
	}
}
