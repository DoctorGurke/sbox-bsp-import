namespace BspImport.Decompiler.Lumps
{
	public class GameLump : BaseLump
	{
		public int Id { get; set; }

		public GameLump( DecompilerContext context, byte[] data, int version = 0 ) : base( context, data, version ) { }

		protected override void Parse( BinaryReader reader, int capacity )
		{
			if ( Context.Data is null )
				return;

			Id = reader.ReadInt32();
			reader.ReadUInt16(); // ushort flags
			reader.ReadUInt16(); // ushort version
			var offset = reader.ReadInt32();
			var length = reader.ReadInt32();

			// offset is based on full file start, aka raw initial data
			var gameLumpData = Context.Data.Take( new Range( offset, offset + length ) ).ToArray();

			switch ( (GameLumpType)Id )
			{
				case GameLumpType.StaticPropLump:
					_ = new StaticPropLump( Context, gameLumpData );
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
