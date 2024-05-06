namespace BspImport.Decompiler;

public class StructReader<T> where T : struct
{
	public byte[] byteBuf;

	public StructReader()
	{
		byteBuf = new byte[Marshal.SizeOf<T>()];
	}

	public IEnumerable<T> ReadMultiple( IEnumerable<byte> bytes, int num = 0 )
	{
		for ( int i = 0; i < num; i++ )
		{
			yield return Read( bytes );
			bytes = bytes.Skip( Marshal.SizeOf<T>() );
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
			Array.Copy( bytes.ToArray(), byteBuf, Marshal.SizeOf<T>() );
			bytes = bytes.Skip( Marshal.SizeOf<T>() ).ToArray();

			object? result;
			GCHandle handle = GCHandle.Alloc( byteBuf, GCHandleType.Pinned );
			try
			{
				result = Marshal.PtrToStructure( handle.AddrOfPinnedObject(), typeof( T ) );
			}
			finally
			{
				handle.Free();
			}

			if ( result is null )
			{
				result = default( T );
			}

			yield return (T)result;
		}
	}

	public T Read( IEnumerable<byte> bytes )
	{
		Array.Copy( bytes.ToArray(), byteBuf, Marshal.SizeOf<T>() );
		if ( bytes.Count() == 0 ) throw new InvalidOperationException( "<EOF>" );

		object? result;
		GCHandle handle = GCHandle.Alloc( byteBuf, GCHandleType.Pinned );
		try
		{
			result = Marshal.PtrToStructure( handle.AddrOfPinnedObject(), typeof( T ) );
		}
		finally
		{
			handle.Free();
		}

		if ( result is null )
		{
			result = default( T );
		}

		return (T)result;
	}
}
