using System;
using System.Collections.Generic;
using System.Text;

namespace BspImport.Decompiler.Lumps;

internal class BrushSideLump : BaseLump
{
	public BrushSideLump( ImportContext context, byte[] data, int version = 0 ) : base( context, data, version )
	{
	}

	protected override void Parse( BinaryReader reader )
	{
		var structReaders = Context.FormatDescriptor.GetStructReaders( Context.BspVersion );
		var brushSideCount = reader.GetLength() / structReaders.BrushSideStructSize;
		Context.BrushSides = new Formats.BrushSide[brushSideCount];

		for ( int i = 0; i < brushSideCount; i++ )
		{
			ushort planeNum = reader.ReadUInt16();
			short texInfo = reader.ReadInt16();
			short dispInfo = reader.ReadInt16();

			bool bevel;
			bool thin = false;

			if ( Context.BspVersion >= 21 )
			{
				bevel = reader.ReadByte() != 0;
				thin = reader.ReadByte() != 0;
			}
			else
			{
				bevel = reader.ReadInt16() != 0;
			}

			var brushSide = new Formats.BrushSide( planeNum, texInfo, dispInfo, bevel, thin );
			Context.BrushSides[i] = brushSide;
		}
	}
}
