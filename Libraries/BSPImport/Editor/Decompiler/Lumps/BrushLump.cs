using System;
using System.Collections.Generic;
using System.Text;

namespace BspImport.Decompiler.Lumps;

public class BrushLump : BaseLump
{
	public BrushLump( ImportContext context, byte[] data, int version = 0 ) : base( context, data, version )
	{
	}

	protected override void Parse( BinaryReader _reader )
	{
		int brushLength = sizeof( int ) * 3;

		int brushCount = _reader.GetLength() / brushLength;

		for ( int i = 0; i < brushCount; i++ )
		{
			var reader = _reader.Split( brushLength );
			int firstSide = reader.ReadInt32();
			int numSides = reader.ReadInt32();
			uint contents = reader.ReadUInt32();
			var contentsFlags = (ContentsFlags)contents;
			if ( (contentsFlags & ContentsFlags.PlayerClip) != 0 )
			{
				//Log.Warning( $"Found clip brush!" );
			}
		}
	}
}
