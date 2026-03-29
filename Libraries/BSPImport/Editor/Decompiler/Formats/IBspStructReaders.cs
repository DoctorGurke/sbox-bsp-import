namespace BspImport.Decompiler.Formats;

/// <summary>
/// Provides format-specific binary readers for BSP data structures.
/// Each BSP game format implements this to handle structural layout differences
/// in its face and leaf binary representations.
/// </summary>
public interface IBspStructReaders
{
	/// <summary>
	/// The byte size of a single face (dface_t) entry in this format.
	/// Used to compute face count from total lump byte length.
	/// </summary>
	int FaceStructSize { get; }

	/// <summary>
	/// The byte size of a single leaf (dleaf_t) entry in this format.
	/// </summary>
	int LeafStructSize { get; }

	/// <summary>
	/// Reads a single face (dface_t) from the binary stream using this format's layout.
	/// Leaves the reader positioned exactly at the start of the next entry.
	/// </summary>
	Face ReadFace( BinaryReader reader );
}
