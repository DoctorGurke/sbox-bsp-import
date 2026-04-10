using System.ComponentModel;
using static System.Runtime.InteropServices.JavaScript.JSType;

namespace BspImport.Decompiler.Lumps;

public class TexDataStringDataLump : BaseLump
{
	public TexDataStringDataLump( ImportContext context, byte[] data, int version = 0 ) : base( context, data, version ) { }

	protected override void Parse( BinaryReader reader )
	{
		var chars = Encoding.ASCII.GetChars( reader.ReadBytes( reader.GetLength() ) );
		var text = new string( chars );

		var texData = new TexDataStringData( text );

		//Log.Info( $"TEXDATASTRINGDATA: {texData.Count}" );

		Context.TexDataStringData = texData;
	}
}

public class TexDataStringData
{
	//private string[] StringList { get; set; }

	[Hide]
	private string Data;

	public TexDataStringData( string data )
	{
		Data = data;
		//var splits = data.Split( '\0', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries );
		//StringList = splits.ToArray();
	}

	public string FromStringTableIndex( int index )
	{
		var end = Data.IndexOf( '\0', index );
		return Data.Substring( index, end - index );
	}
}
