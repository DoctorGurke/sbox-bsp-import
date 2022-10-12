namespace BspImport.Decompiler.Lumps;

public class TexDataStringDataLump : BaseLump
{
	public TexDataStringDataLump( ImportContext context, byte[] data, int version = 0 ) : base( context, data, version ) { }

	protected override void Parse( BinaryReader reader )
	{
		var chars = Encoding.ASCII.GetChars( reader.ReadBytes( reader.GetLength() ) );
		var text = new string( chars );

		var texData = new TexDataStringData( text );

		Log.Info( $"TEXDATASTRINGDATA: {texData.Count}" );

		Context.TexDataStringData = texData;
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
}
