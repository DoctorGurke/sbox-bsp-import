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

	/// <summary>The BSP version number in the file header for this format.</summary>
	int BspVersion { get; }

	/// <summary> Human-readable name used in log messages and the import context. </summary>
	string DisplayName { get; }

	/// <summary>
	/// When <c>true</c>, the BSP version number alone uniquely identifies this format,
	/// no entity-based disambiguation is needed.
	/// When <c>false</c>, <see cref="MatchesEntities"/> may be called for disambiguation.
	/// </summary>
	bool IsDefinitiveForVersion { get; }

	/// <summary>
	/// Secondary identification using entity classnames from lump 0.
	/// Only called when multiple descriptors share the same version number.
	/// Return <c>true</c> if the provided entity composition is consistent with this format.
	/// </summary>
	/// <param name="entityClassNames">
	/// Distinct classnames of all entities parsed from lump 0.
	/// </param>
	bool MatchesEntities( IReadOnlyList<string> entityClassNames );

	/// <summary>Format-specific binary struct readers.</summary>
	IBspStructReaders StructReaders { get; }
}
