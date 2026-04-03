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

	/// <summary>
	/// Reads a single brush side (dbrushside_t) from the binary stream using this format's layout.
	/// </summary>
	//BrushSide ReadBrushSide( BinaryReader reader, IBspFormatDescriptor format );
	BrushSide ReadBrushSide( BinaryReader reader );

	/// <summary>
	/// Returns the full byte size of one static prop entry for the given game lump version.
	/// </summary>
	int GetStaticPropEntrySize( int staticPropVersion );

	/// <summary>
	/// Reads a single static prop instance using the active format descriptor and lump version.
	/// The reader must consume the full entry size for the resolved static prop layout.
	/// </summary>
	StaticPropInstance ReadStaticProp(
		BinaryReader reader,
		IBspFormatDescriptor format,
		int staticPropVersion );
}

public readonly struct BrushSide
{
	public ushort PlaneNum { get; }
	public short TexInfo { get; }
	public short DispInfo { get; }
	public bool Bevel { get; }
	public bool Thin { get; }


	public BrushSide( ushort planeNum, short texInfo, short dispInfo, bool bevel, bool thin = false )
	{
		PlaneNum = planeNum;
		TexInfo = texInfo;
		DispInfo = dispInfo;
		Bevel = bevel;
		Thin = thin;
	}
}

public readonly struct StaticPropInstance( Vector3 origin, Vector3 angles, ushort propType )
{
	public Vector3 Origin { get; } = origin;
	public Vector3 Angles { get; } = angles;
	public ushort PropType { get; } = propType;
}
