﻿namespace BspImport.Decompiler.Lumps;

public abstract class BaseLump
{
	protected DecompilerContext Context { get; set; }
	public int Version { get; private set; }
	protected byte[] Data { get; private set; }

	public BaseLump( DecompilerContext context, byte[] data, int version = 0 )
	{
		Context = context;
		Data = data;
		Version = version;

		var bReader = new BinaryReader( new MemoryStream( data ) );
		Parse( bReader, data.Length );
	}

	protected abstract void Parse( BinaryReader reader, int capacity );
}
