﻿using System.Runtime.InteropServices;

namespace BspImport.Decompiler;

public class StructReader<T> where T : struct
{
	public byte[] byteBuf;

	public StructReader()
	{
		byteBuf = new byte[Marshal.SizeOf( typeof( T ) )];
	}

	public IEnumerable<T> ReadMultiple( IEnumerable<byte> bytes, int num = 0 )
	{
		for ( int i = 0; i < num; i++ )
		{
			yield return Read( bytes );
			bytes = bytes.Skip( Marshal.SizeOf( typeof( T ) ) );
		}
	}

	public IEnumerable<T> ReadMultiple( IEnumerable<byte> bytes )
	{
		if ( bytes.Count() == 0 ) throw new ArgumentException( "No Data" );
		// input array length is multiple of base array length, to make sure it's a collection of our data type
		if ( bytes.Count() % byteBuf.Length != 0 )
		{
			throw new ArgumentException( $"Bad Data. Expected multiple of {byteBuf.Length}, got {bytes.Count()}." );
		}

		while ( bytes.Count() >= byteBuf.Length )
		{
			Array.Copy( bytes.ToArray(), byteBuf, Marshal.SizeOf( typeof( T ) ) );
			bytes = bytes.Skip( Marshal.SizeOf( typeof( T ) ) ).ToArray();

			T result;
			GCHandle handle = GCHandle.Alloc( byteBuf, GCHandleType.Pinned );
			try
			{
				result = (T)Marshal.PtrToStructure( handle.AddrOfPinnedObject(), typeof( T ) );
			}
			finally
			{
				handle.Free();
			}
			yield return result;
		}
	}

	public T Read( IEnumerable<byte> bytes )
	{
		Array.Copy( bytes.ToArray(), byteBuf, Marshal.SizeOf( typeof( T ) ) );
		if ( bytes.Count() == 0 ) throw new InvalidOperationException( "<EOF>" );

		T result;
		GCHandle handle = GCHandle.Alloc( byteBuf, GCHandleType.Pinned );
		try
		{
			result = (T)Marshal.PtrToStructure( handle.AddrOfPinnedObject(), typeof( T ) );
		}
		finally
		{
			handle.Free();
		}
		return result;
	}
}
