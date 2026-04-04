namespace BspImport.Decompiler.Formats.Readers;

/// <summary>
/// Struct readers for standard Source Engine BSP formats.
/// Standard dface_t is 56 bytes, dleaf_t is 32 bytes, and dbrushside_t uses the
/// standard Source layout. Static prop entry layout depends on lump version.
/// </summary>
public sealed class StandardBspStructReaders : IBspStructReaders
{
	public int FaceStructSize => 56;
	public int LeafStructSize => 32;
	public int BrushSideStructSize => 8;

	public Face ReadFace( BinaryReader reader ) => reader.ReadFace();

	public BrushSide ReadBrushSide( BinaryReader reader )
	{
		ushort planeNum = reader.ReadUInt16();
		short texInfo = reader.ReadInt16();
		short dispInfo = reader.ReadInt16();
		bool bevel = reader.ReadByte() != 0;
		reader.ReadByte(); // padding / unused

		return new BrushSide( planeNum, texInfo, dispInfo, bevel );
	}

	public int GetStaticPropEntrySize( int staticPropVersion )
	{
		return staticPropVersion switch
		{
			4 => 56,
			5 => 60,
			6 => 64,
			7 => 68,
			8 => 68,
			9 => 72,
			10 => 76,
			11 => 80,
			_ => throw new InvalidOperationException(
				$"Unsupported static prop version {staticPropVersion}." )
		};
	}

	public StaticPropInstance ReadStaticProp(
		BinaryReader reader,
		IBspFormatDescriptor format,
		int staticPropVersion )
	{
		//int entrySize = ResolveStaticPropEntrySize( format, staticPropVersion );
		//using var sprp = reader.Split( entrySize );

		Vector3 origin = reader.ReadVector3();
		Vector3 angles = reader.ReadVector3();
		ushort propType = reader.ReadUInt16();

		return new StaticPropInstance( origin, angles, propType );
	}

	private int ResolveStaticPropEntrySize( IBspFormatDescriptor format, int staticPropVersion )
	{
		if ( staticPropVersion > 0 )
			return GetStaticPropEntrySize( staticPropVersion );

		return format.StaticPropLayout switch
		{
			StaticPropLayout.V4 => 56,
			StaticPropLayout.V5 => 60,
			StaticPropLayout.V6 => 64,
			StaticPropLayout.V7 => 68,
			StaticPropLayout.V7Xbox => 72,
			StaticPropLayout.V8 => 68,
			StaticPropLayout.V9 => 72,
			StaticPropLayout.V10 => 76,
			StaticPropLayout.V11 => 80,
			_ => throw new InvalidOperationException(
				$"Unsupported static prop layout {format.StaticPropLayout} for {format.DisplayName}." )
		};
	}
}
