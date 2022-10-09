using BspImport.Decompiler.Lumps;

namespace BspImport.Decompiler;

public partial class MapDecompiler
{
	protected virtual BaseLump? ParseLump( int index, IEnumerable<byte> data, int version )
	{
		switch ( (LumpType)index )
		{
			case LumpType.EntityLump:
				return new EntityLump( Context, data );
			case LumpType.ModelLump:
				return new ModelLump( Context, data );
			case LumpType.GameLump:
				return new GameLumpHeader( Context, data );
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

