using BspImport.Decompiler;
using BspImport.Decompiler.Lumps;
using System.Runtime.InteropServices;

namespace BspImport.Extensions;

public static class BinaryReaderX
{
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
		reader.ReadInt16(); // short surfaceFogVolumeID
		reader.ReadBytes( 4 ); // byte styles[4]
		reader.ReadInt32(); // int lightofs
		reader.ReadSingle(); // float area
		reader.ReadBytes( sizeof( int ) * 2 ); // int LightmapTextureMinsInLuxels[2]
		reader.ReadBytes( sizeof( int ) * 2 ); // int LightmapTextureSizeInLuxels[2]

		var oFace = reader.ReadInt32();

		// don't need this either, but need to get rid of the padding
		reader.ReadUInt16(); // ushort numPrims
		reader.ReadUInt16(); // ushort firstPrimID
		reader.ReadUInt32(); // uint smoothingGroups

		return new Face( firstEdge, numEdges, texInfo, dispInfo, oFace );
	}

	public static BinaryReader Split( this BinaryReader current, int length ) => new BinaryReader( new MemoryStream( current.ReadBytes( length ) ) );

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
