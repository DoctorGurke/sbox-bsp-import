using LZMA = SevenZip.Compression.LZMA;

namespace BspImport.Decompiler.Lumps;

public partial class BaseLump
{
	/// <summary>
	/// LZMA magic number.
	/// </summary>
	private const uint LZMA_ID = (('A' << 24) | ('M' << 16) | ('Z' << 8) | ('L'));

	/// <summary>
	/// Checks for the LZMA magic number in given Byte data.
	/// </summary>
	/// <param name="data">Byte data to check for LZMA compression magic number.</param>
	/// <returns>True if the first 4 bytes represent the chars 'LZMA', false otherwise.</returns>
	private bool IsCompressed( byte[] data )
	{
		var reader = new BinaryReader( new MemoryStream( data ) );
		var lzmaId = reader.ReadUInt32();

		return lzmaId == LZMA_ID;
	}

	/// <summary>
	/// Decompress the given LZMA compressed byte data.
	/// </summary>
	/// <param name="inData">Byte array to decompres.s</param>
	/// <returns>Decompressed byte array.</returns>
	private byte[] Decompress( byte[] inData )
	{
		// turn source lzma to standard lzma, constructs and stitches standard lzma header
		var standardLzma = ConstructStandardLzma( inData );

		// standard lzma format
		var reader = new BinaryReader( new MemoryStream( standardLzma ) );

		// get lzma properties
		byte[] properties = new byte[5];
		if ( reader.Read( properties, 0, 5 ) != 5 )
			throw (new Exception( "Unable to read lzma properties!" ));

		// setup decoder
		var decoder = new LZMA.Decoder();
		decoder.SetDecoderProperties( properties );

		// body size before and after compression
		long uncompressedSize = reader.ReadInt64();
		long compressedSize = reader.GetLength();

		// decompress and put into byte array
		var outStream = new MemoryStream();

		decoder.Code( reader.BaseStream, outStream, compressedSize, uncompressedSize, null );

		//var outData = outStream.ReadByteArrayFromStream( 0, (uint)outStream.Length );

		return outStream.ToArray();
	}

	// this sucks
	private byte[] ConstructStandardLzma( byte[] sourceLzma )
	{
		// read source engine lzma header
		var sourceHeaderReader = new StructReader<SourceLzmaHeader>();
		var sourceHeaderLength = Marshal.SizeOf<SourceLzmaHeader>();
		var sourceHeader = sourceHeaderReader.Read( sourceLzma );

		// take lzma body only
		var lzmaBody = sourceLzma[sourceHeaderLength..];

		var lzmaHeaderLength = Marshal.SizeOf<LzmaHeader>();

		// construct standard lzma
		var standardLzma = new byte[lzmaHeaderLength + lzmaBody.Length];
		// properties
		sourceHeader.Properties.CopyTo( standardLzma, 0 );
		// actual size
		BitConverter.GetBytes( (ulong)sourceHeader.ActualSize ).CopyTo( standardLzma, 5 );
		// body
		lzmaBody.CopyTo( standardLzma, lzmaHeaderLength );

		return standardLzma;
	}

	/// <summary>
	/// LZMA header as used by source bsp formats.
	/// </summary>
	[StructLayout( LayoutKind.Sequential, Size = 17, Pack = 1 )]
	private struct SourceLzmaHeader
	{
		public uint Id;             // 4
		public uint ActualSize;     // 4
		public uint LzmaSize;       // 4
		[MarshalAs( UnmanagedType.ByValArray, SizeConst = 5 )]
		public byte[] Properties;   // 5
	}

	/// <summary>
	/// Standard LZMA header as used by the LZMA sdk.
	/// </summary>
	[StructLayout( LayoutKind.Sequential, Size = 13, Pack = 1 )]
	private struct LzmaHeader
	{
		[MarshalAs( UnmanagedType.ByValArray, SizeConst = 5 )]
		public byte[] Properties;   // 5
		public ulong ActualSize;    // 8
	}
}
