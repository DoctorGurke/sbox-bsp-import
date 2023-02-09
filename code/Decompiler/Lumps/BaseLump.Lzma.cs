using System.IO;
using SevenZip;

namespace BspImport.Decompiler.Lumps;

public partial class BaseLump
{
	private const uint LZMA_ID = (('A' << 24) | ('M' << 16) | ('Z' << 8) | ('L'));

	private bool IsCompressed( byte[] data )
	{
		var reader = new BinaryReader( new MemoryStream( data ) );
		var lzmaId = reader.ReadUInt32();

		var compressed = lzmaId == LZMA_ID;

		if ( compressed )
		{
			var decompressedData = Decompress( data );
		}

		return compressed;
	}

	private byte[] Decompress( byte[] inData )
	{
		var sourceHeaderReader = new StructReader<SourceLzmaHeader>();
		var sourceHeader = sourceHeaderReader.Read( inData );
		var sourceHeaderLength = 17;

		var lzmaHeader = new LzmaHeader( sourceHeader );
		var lzmaHeaderLength = 13;

		// lzma body length
		var bodyLength = inData.Length - sourceHeaderLength;

		// take lzma body only
		var lzmaBody = inData[sourceHeaderLength..];

		// construct standard lzma
		var standardLzma = new byte[lzmaHeaderLength + bodyLength];
		// properties
		sourceHeader.Properties.CopyTo( standardLzma, 0 );
		// actual size
		BitConverter.GetBytes( (ulong)sourceHeader.ActualSize ).CopyTo( standardLzma, 5 );
		// body
		lzmaBody.CopyTo( standardLzma, lzmaHeaderLength );

		// standard lzma format
		var reader = new BinaryReader( new MemoryStream( standardLzma ) );

		byte[] properties = new byte[5];
		if ( reader.Read( properties, 0, 5 ) != 5 )
			throw (new Exception( "Unable to read lzma properties!" ));

		var decoder = new SevenZip.Compression.LZMA.Decoder();
		decoder.SetDecoderProperties( properties );

		long outSize = reader.ReadInt64();

		long compressedSize = reader.GetLength();

		// decompress
		var outStream = new MemoryStream();
		decoder.Code( reader.BaseStream, outStream, compressedSize, outSize, null );
		var outData = outStream.ReadByteArrayFromStream( 0, (uint)outStream.Length );

		if ( outStream.Length == outSize )
		{
			//Log.Info( $"decompressed successfully" );
		}

		return outData;
	}

	/* lzma decoder properties
	 * 
	 * dict size in bytes
	 * lc - number of high bits of the previous byte to use as a context for literal decoding
	 * lp - number of low bits of the dict position to include in literal position state
	 * pb - number of low bits of the dict position to include in position state
	 */

	/* source lzma header total: 17
	 * 
	 * uint id
	 * uint actualSize
	 * uint lzmaSize
	 * char[5] properties
	 */


	[StructLayout( LayoutKind.Sequential )]
	private struct SourceLzmaHeader
	{
		public uint Id;             // 4
		public uint ActualSize;     // 4
		public uint LzmaSize;       // 4
		[MarshalAs( UnmanagedType.ByValArray, SizeConst = 5 )]
		public byte[] Properties;   // 5

		//public override string ToString()
		//{
		//	return $"SOURCE LZMA HEADER: actual Size: {ActualSize} lzma Size: {LzmaSize} properties: {BitConverter.ToString( Properties )}\0";
		//}
	}

	/* standard lzma header total: 13
	 * 
	 * char[5] properties;
	 * ulong actualSize;
	 */
	[StructLayout( LayoutKind.Sequential )]
	private struct LzmaHeader
	{
		public byte[] Properties;   // 5
		public ulong ActualSize;    // 8

		public LzmaHeader( SourceLzmaHeader header )
		{
			Properties = header.Properties;
			ActualSize = header.ActualSize;
		}

		//public override string ToString()
		//{
		//	return $"LZMA HEADER: properties: {BitConverter.ToString( Properties )} actual Size: {ActualSize}";
		//}
	}

	//private static byte[] LzmaHeaderToByteArray( LzmaHeader header )
	//{
	//	int size = 13;//Marshal.SizeOf( header );
	//	byte[] arr = new byte[size];

	//	IntPtr ptr = IntPtr.Zero;
	//	try
	//	{
	//		ptr = Marshal.AllocHGlobal( size );
	//		Marshal.StructureToPtr( header, ptr, true );
	//		Marshal.Copy( ptr, arr, 0, size );
	//	}
	//	finally
	//	{
	//		Marshal.FreeHGlobal( ptr );
	//	}
	//	return arr;
	//}
}
