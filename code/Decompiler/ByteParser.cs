using System.Runtime.InteropServices;

namespace BspImport.Decompiler;

public class ByteParser
{
	public IEnumerable<byte> Data { get; set; }
	private IEnumerable<byte> _buffer;

	private int SizeOf( Type t ) => Marshal.SizeOf( t );

	public ByteParser( IEnumerable<byte> data )
	{
		Data = data;
		_buffer = data;
	}

	public static implicit operator byte[]( ByteParser p ) => p._buffer.ToArray();

	/// <summary>
	/// Get the remainding capacity of the byte buffer.
	/// </summary>
	public int BufferCapacity => _buffer.Count();

	/// <summary>
	/// Read n bytes from the buffer.
	/// </summary>
	/// <param name="num">Number of bytes to read.</param>
	/// <returns>IEnumerable of bytes with n length.</returns>
	public IEnumerable<byte> ReadBytes( int num )
	{
		var result = _buffer.Take( num );
		_buffer = _buffer.Skip( num );

		return result;
	}

	/// <summary>
	/// Skips this many bytes in the parser buffer.
	/// </summary>
	/// <param name="num">Amount of bytes to skip.</param>
	public void Skip( int num )
	{
		_buffer = _buffer.Skip( num );
	}

	/// <summary>
	/// Skip a specific type n times, default 1.
	/// </summary>
	/// <typeparam name="T">Type.</typeparam>
	/// <param name="num">Number of times to skip.</param>
	public void Skip<T>( int num = 1 )
	{
		Skip( SizeOf( typeof( T ) ) * num );
	}

	/// <summary>
	/// Read a type from the buffer.
	/// </summary>
	/// <typeparam name="T">Type.</typeparam>
	/// <returns>Type instance parsed from the byte buffer.</returns>
	/// <exception cref="Exception">Throws if trying to read beyond buffer capacity.</exception>
	public T Read<T>() where T : struct
	{
		var size = SizeOf( typeof( T ) );

		if ( _buffer.Count() < size )
			throw new Exception( $"Type size exceeds ByteParser buffer size!" );

		var reader = new StructReader<T>();
		var result = reader.Read( _buffer.ToArray() );

		_buffer = _buffer.Skip( size );

		return result;
	}

	/// <summary>
	/// Read a specific type n times, default 1.
	/// </summary>
	/// <typeparam name="T">Type.</typeparam>
	/// <param name="num">Number of times to read.</param>
	/// <returns>IEnumerable of Type with n elements.</returns>
	/// <exception cref="Exception">Throws if trying to read beyond buffer capacity.</exception>
	public IEnumerable<T> Read<T>( int num ) where T : struct
	{
		var size = SizeOf( typeof( T ) );

		if ( _buffer.Count() < size * num )
			throw new Exception( $"Type size exceeds ByteParser buffer size!" );

		for ( int i = 0; i < num; i++ )
		{
			yield return Read<T>();
			_buffer = _buffer.Skip( SizeOf( typeof( T ) ) );
		}
	}

	/// <summary>
	/// Tries to read as many instances of the provided type as possible.
	/// </summary>
	/// <typeparam name="T">Type.</typeparam>
	/// <returns>IEnumerable of type instances read from byte buffer.</returns>
	/// <exception cref="Exception">Throws if reading exceeds buffer capacity.</exception>
	public IEnumerable<T> TryReadMultiple<T>() where T : struct
	{
		var size = SizeOf( typeof( T ) );

		if ( _buffer.Count() < size )
			throw new Exception( $"Type size exceeds ByteParser buffer size!" );

		var reader = new StructReader<T>();
		var result = reader.ReadMultiple( _buffer );

		_buffer = _buffer.Skip( size );

		return result;
	}
}
