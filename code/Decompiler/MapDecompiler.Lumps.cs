using BspImport.Decompiler.Lumps;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BspImport.Decompiler;

public partial class MapDecompiler
{
	private static BaseLump? ParseLump( int index, IEnumerable<byte> data, int version )
	{
		switch ( (LumpType)index )
		{
			case LumpType.EntityLump:
				return new EntityLump( data );
			case LumpType.ModelLump:
				return new ModelLump( data );
			case LumpType.GameLump:
				return new GameLumpHeader( data );
			default:
				break;
		}
		return null;
	}

	public enum LumpType
	{
		EntityLump = 0,
		ModelLump = 14,
		GameLump = 35,
	}
}

