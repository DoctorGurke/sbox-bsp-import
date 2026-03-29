namespace BspImport.Decompiler.Formats.Readers;

/// <summary>
/// Struct readers for standard Source Engine BSP formats (v20, v21, v22).
///
/// Standard dface_t layout (56 bytes)
/// </summary>
public sealed class StandardBspStructReaders : IBspStructReaders
{
	public int FaceStructSize => 56;
	public int LeafStructSize => 32;

	/// <summary>
	/// Delegates to <see cref="BinaryReaderX.ReadFace"/> as the existing extension method
	/// already implements the standard 56-byte face layout. No duplication needed.
	/// </summary>
	public Face ReadFace( BinaryReader reader ) => reader.ReadFace();
}
