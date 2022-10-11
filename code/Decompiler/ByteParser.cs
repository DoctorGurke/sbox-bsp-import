﻿using System.Runtime.InteropServices;

namespace BspImport.Decompiler;

public class ByteParser
{
	public byte[] Data { get; private set; }
	private byte[] _buffer;

	private int SizeOf( Type t ) => Marshal.SizeOf( t );

	public ByteParser( byte[] data )
	{
		Data = data;
		_buffer = data;
	}

	/// <summary>
	/// Get the remainding capacity of the byte buffer.
	/// </summary>
	public int BufferCapacity => _buffer.Count();

	/// <summary>
	/// Implicitly convert the ByteParser to its current byte buffer.
	/// </summary>
	/// <param name="p">ByteParser.</param>
	public static implicit operator byte[]( ByteParser p ) => p._buffer.ToArray();

	/// <summary>
	/// Skips this many bytes in the parser buffer.
	/// </summary>
	/// <param name="num">Amount of bytes to skip.</param>
	/// /// <param name="condition">Whether to skip or not.</param>
	public void Skip( int num, bool condition = true )
	{
		if ( !condition )
			return;

		_buffer = _buffer.Skip( num ).ToArray();
	}

	/// <summary>
	/// Skip a specific type n times, default 1.
	/// </summary>
	/// <typeparam name="T">Type.</typeparam>
	/// <param name="num">Number of times to skip.</param>
	/// <param name="condition">Whether to skip or not.</param>
	public void Skip<T>( int num = 1, bool condition = true )
	{
		Skip( SizeOf( typeof( T ) ) * num, condition );
	}

	/// <summary>
	/// Read n bytes from the buffer.
	/// </summary>
	/// <param name="num">Number of bytes to read.</param>
	/// <param name="skip">Whether to skip/progress the buffer or not.</param>
	/// <returns>IEnumerable of bytes with n length.</returns>
	public byte[] ReadBytes( int num, bool skip = true )
	{
		var array = _buffer.Take( num ).ToArray();

		Skip( num, skip );

		return array;
	}

	/// <summary>
	/// Like ReadBytes but does not skip buffer.
	/// </summary>
	/// <param name="num">Number of bytes to copy.</param>
	/// <returns>Collection of bytes.</returns>
	public byte[] CopyBytes( int num )
	{
		var array = _buffer.Take( num ).ToArray();
		return array;
	}

	/// <summary>
	/// Copy an elementy from the buffer without skipping/progressing it.
	/// </summary>
	/// <typeparam name="T">Type.</typeparam>
	/// <returns>The copied element.</returns>
	public T Copy<T>() where T : struct
	{
		return Read<T>( false );
	}

	/// <summary>
	/// Read a type from the buffer.
	/// </summary>
	/// <typeparam name="T">Type.</typeparam>
	/// <returns>Type instance parsed from the byte buffer.</returns>
	/// <exception cref="Exception">Throws if trying to read beyond buffer capacity.</exception>
	public T Read<T>( bool skip = true ) where T : struct
	{
		var size = SizeOf( typeof( T ) );

		if ( _buffer.Count() < size )
			throw new Exception( $"Type size exceeds ByteParser buffer size!" );

		var reader = new StructReader<T>();
		var result = reader.Read( _buffer.ToArray() );

		Skip( size, skip );

		return result;
	}

	/// <summary>
	/// Read a specific type n times, default 1.
	/// </summary>
	/// <typeparam name="T">Type.</typeparam>
	/// <param name="num">Number of times to read.</param>
	/// <returns>IEnumerable of Type with n elements.</returns>
	/// <exception cref="Exception">Throws if trying to read beyond buffer capacity.</exception>
	public T[] Read<T>( int num, bool skip = true ) where T : struct
	{
		var size = SizeOf( typeof( T ) );
		var array = new T[num];

		if ( _buffer.Count() < size * num )
			throw new Exception( $"Type size exceeds ByteParser buffer size!" );

		for ( int i = 0; i < num; i++ )
		{
			array[i] = Read<T>();

			Skip( size, skip );
		}

		return array;
	}

	/// <summary>
	/// Tries to read as many instances of the provided type as possible.
	/// </summary>
	/// <typeparam name="T">Type.</typeparam>
	/// <returns>IEnumerable of type instances read from byte buffer.</returns>
	/// <exception cref="Exception">Throws if reading exceeds buffer capacity.</exception>
	public T[] TryReadMultiple<T>( bool skip = true ) where T : struct
	{
		var size = SizeOf( typeof( T ) );

		if ( _buffer.Count() < size )
			throw new Exception( $"Type size exceeds ByteParser buffer size!" );

		var reader = new StructReader<T>();
		var result = reader.ReadMultiple( _buffer );

		Skip( size, skip );

		return result.ToArray();
	}
}
