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

		return lzmaId == LZMA_ID;
	}

	private byte[] Decompress( byte[] inData )
	{
		var sourceHeaderReader = new StructReader<SourceLzmaHeader>();
		var sourceHeader = sourceHeaderReader.Read( inData );
		var sourceHeaderLength = Marshal.SizeOf<SourceLzmaHeader>();

		var lzmaHeaderLength = Marshal.SizeOf<LzmaHeader>();

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

		return outData;
	}

	[StructLayout( LayoutKind.Sequential, Size = 17, Pack = 1 )]
	private struct SourceLzmaHeader
	{
		public uint Id;             // 4
		public uint ActualSize;     // 4
		public uint LzmaSize;       // 4
		[MarshalAs( UnmanagedType.ByValArray, SizeConst = 5 )]
		public byte[] Properties;   // 5
	}

	[StructLayout( LayoutKind.Sequential, Size = 13, Pack = 1 )]
	private struct LzmaHeader
	{
		[MarshalAs( UnmanagedType.ByValArray, SizeConst = 5 )]
		public byte[] Properties;   // 5
		public ulong ActualSize;    // 8
	}
}
