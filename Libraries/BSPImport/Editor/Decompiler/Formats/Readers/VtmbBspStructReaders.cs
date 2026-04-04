namespace BspImport.Decompiler.Formats.Readers;

/// <summary>
/// Struct readers for Vampire: The Masquerade – Bloodlines (BSP v17).
///
/// dface_t layout for Bloodlines v17 (104 bytes):
/// </summary>
public sealed class VtmbBspStructReaders : IBspStructReaders
{
	public int FaceStructSize => 104;
	public int LeafStructSize => 32;
	public int BrushSideStructSize => 8;

	public Face ReadFace( BinaryReader reader )
	{
		// ------------------------------------------------------------------
		// Lighting prefix: colorRGBExp32 m_AvgLightColor[8]
		// Each colorRGBExp32 = byte r, byte g, byte b, byte exponent = 4 bytes.
		// 8 entries × 4 bytes = 32 bytes.
		// ------------------------------------------------------------------
		reader.Skip( 32 );

		// Standard identification fields (same offsets as v20 after the prefix).
		reader.ReadUInt16();              // planenum
		reader.ReadByte();                // side
		reader.ReadByte();                // onNode

		int firstEdge = reader.ReadInt32();
		short numEdges = reader.ReadInt16();
		short texInfo = reader.ReadInt16();
		short dispInfo = reader.ReadInt16();

		short surfaceFogVolumeID = reader.ReadInt16();             // surfaceFogVolumeID

		// MAXLIGHTMAPS=8 in VTMB: styles[8] + day[8] + night[8] = 24 bytes.
		// Standard v20 only has styles[4] = 4 bytes.
		reader.Skip( 8 );                 // styles[8]
		reader.Skip( 8 );                 // day[8]   (nighttime lightmapping system)
		reader.Skip( 8 );                 // night[8] (nighttime lightmapping system)

		reader.Skip<int>();               // lightofs

		float area = reader.ReadSingle();

		reader.Skip<int>( 2 );            // m_LightmapTextureMinsInLuxels[2]
		reader.Skip<int>( 2 );            // m_LightmapTextureSizeInLuxels[2]

		int oFace = reader.ReadInt32();

		reader.Skip<uint>();              // smoothingGroups

		// No numPrims / firstPrimID as those fields were added in v20+.
		return new Face( firstEdge, numEdges, texInfo, dispInfo, surfaceFogVolumeID, area, oFace );
	}

	public BrushSide ReadBrushSide( BinaryReader reader )
		=> throw new NotSupportedException( "VTMB brush side parsing is not implemented yet." );

	public int GetStaticPropEntrySize( int staticPropVersion )
		=> throw new NotSupportedException( "VTMB static prop parsing is not implemented yet." );

	public StaticPropInstance ReadStaticProp( BinaryReader reader, IBspFormatDescriptor format,
		int staticPropVersion )
		=> throw new NotSupportedException( "VTMB static prop parsing is not implemented yet." );
}
