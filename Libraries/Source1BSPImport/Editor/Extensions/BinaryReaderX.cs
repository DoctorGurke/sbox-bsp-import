namespace BspImport.Extensions;

public static class BinaryReaderX
{
	/// <summary>
	/// Get length left in reader.
	/// </summary>
	/// <param name="reader"></param>
	/// <returns></returns>
	public static int GetLength( this BinaryReader reader ) => (int)(reader.BaseStream.Length - reader.BaseStream.Position);

	public static Face ReadFace( this BinaryReader reader )
	{
		reader.ReadUInt16(); // planenum
		reader.ReadByte(); // side
		reader.ReadByte(); // onNode

		var firstEdge = reader.ReadInt32();
		var numEdges = reader.ReadInt16();
		var texInfo = reader.ReadInt16();
		var dispInfo = reader.ReadInt16();

		// don't need any of this
		reader.Skip<short>(); // short surfaceFogVolumeID
		reader.Skip( 4 ); // byte styles[4]
		reader.Skip<int>(); // int lightofs

		var area = reader.ReadSingle();

		reader.Skip<int>( 2 ); // int LightmapTextureMinsInLuxels[2]
		reader.Skip<int>( 2 ); // int LightmapTextureSizeInLuxels[2]

		var oFace = reader.ReadInt32();

		// don't need this either, but need to get rid of the padding
		reader.Skip<ushort>(); // ushort numPrims
		reader.Skip<ushort>(); // ushort firstPrimID
		reader.Skip<uint>(); // uint smoothingGroups

		return new Face( firstEdge, numEdges, texInfo, dispInfo, area, oFace );
	}

	/// <summary>
	/// Split of a whole section of a BinaryReader to treat isolated from original reader. Useful for reading an array of complex data types with useless trailing info.
	/// </summary>
	/// <param name="current">Original binary reader.</param>
	/// <param name="length">Number of bytes to split off.</param>
	/// <returns>Split off section of the original reader as a new binary reader instance.</returns>
	public static BinaryReader Split( this BinaryReader current, int length ) => new BinaryReader( new MemoryStream( current.ReadBytes( length ) ) );

	public static Vector3 ReadVector3( this BinaryReader reader )
	{
		var size = Marshal.SizeOf( typeof( Vector3 ) );
		var sReader = new StructReader<Vector3>();
		return sReader.Read( reader.ReadBytes( size ) );
	}

	public static Vector4 ReadVector4( this BinaryReader reader )
	{
		var size = Marshal.SizeOf( typeof( Vector4 ) );
		var sReader = new StructReader<Vector4>();
		return sReader.Read( reader.ReadBytes( size ) );
	}

	public static void Skip( this BinaryReader reader, int num = 1 )
	{
		reader.ReadBytes( num );
	}

	public static void Skip<T>( this BinaryReader reader, int num = 1 ) where T : struct
	{
		var size = Marshal.SizeOf( typeof( T ) );
		reader.Skip( size * num );
	}
}
