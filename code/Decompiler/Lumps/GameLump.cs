namespace BspImport.Decompiler.Lumps
{
	public class GameLump : BaseLump
	{
		public int Id { get; set; }

		public GameLump( DecompilerContext context, IEnumerable<byte> data, int version = 0 ) : base( context, data, version ) { }

		protected override void Parse( ByteParser data )
		{
			if ( Context.Data is null )
				return;

			Id = data.Read<int>();
			data.Skip<ushort>(); // flags
			data.Skip<ushort>(); // version
			var offset = data.Read<int>();
			var length = data.Read<int>();

			// offset is based on full file start, aka raw initial data
			var gamelumpdata = Context.Data.Take( new Range( offset, offset + length ) );

			switch ( (GameLumpType)Id )
			{
				case GameLumpType.StaticPropLump:
					_ = new StaticPropLump( Context, gamelumpdata );
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
