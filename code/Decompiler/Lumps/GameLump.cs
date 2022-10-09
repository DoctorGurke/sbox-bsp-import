using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BspImport.Decompiler.Lumps
{
	public class GameLump : BaseLump
	{
		public int Id { get; set; }

		public GameLump( DecompilerContext context, IEnumerable<byte> data, int version = 0 ) : base( context, data, version )
		{
			if ( Context.Data is null )
				return;

			var parser = new ByteParser( data );
			Id = parser.Read<int>();
			parser.Skip<ushort>(); // flags
			parser.Skip<ushort>(); // version
			var offset = parser.Read<int>();
			var length = parser.Read<int>();

			// offset is based on full file start, aka raw initial data
			var gamelumpdata = Context.Data.Take( new Range( offset, offset + length ) );

			switch ( (GameLumpType)Id )
			{
				case GameLumpType.StaticPropLump:
					var staticprop = new StaticPropLump( Context, gamelumpdata );
					break;
			}
		}

		private enum GameLumpType
		{
			StaticPropLump = 1936749168,
			DetailPropLump = 1685090928
		}
	}
}
