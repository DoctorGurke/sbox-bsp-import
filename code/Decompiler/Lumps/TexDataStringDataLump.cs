﻿namespace BspImport.Decompiler.Lumps;

public class TexDataStringDataLump : BaseLump
{
	public TexDataStringDataLump( DecompilerContext context, IEnumerable<byte> data, int version = 0 ) : base( context, data, version ) { }

	protected override void Parse( ByteParser data )
	{
		var chars = Encoding.ASCII.GetChars( data );
		var text = new string( chars );

		var texdata = new TexDataStringData( text );

		Log.Info( $"TEXDATASTRINGDATA: {texdata.Count}" );

		Context.TexDataStringData = texdata;
	}
}

public struct TexDataStringData
{
	private string Data;

	public TexDataStringData( string data )
	{
		Data = data;
	}

	public string FromStringTableIndex( int index )
	{
		var end = Data.IndexOf( '\0', index );
		return Data.Substring( index, end - index );
	}

	public int Count => Data.Split( '\0' ).Count();

	public override string ToString() => Data.Replace( '\0', ' ' );
}
