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

	public IEnumerable<byte> ReadBytes( int num )
	{
		var result = _buffer.Take( num );
		_buffer = _buffer.Skip( num );

		return result;
	}

	public void Skip( int num )
	{
		_buffer = _buffer.Skip( num );
	}

	public void Skip<T>()
	{
		Skip( SizeOf( typeof( T ) ) );
	}

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

	public IEnumerable<T> ReadMultiple<T>( int num ) where T : struct
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
