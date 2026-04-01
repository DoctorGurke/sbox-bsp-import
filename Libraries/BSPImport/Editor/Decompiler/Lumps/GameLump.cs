namespace BspImport.Decompiler.Lumps
{
	public class GameLump : BaseLump
	{
		public int Id { get; set; }

		public GameLump( ImportContext context, byte[] data, int version = 0 ) : base( context, data, version ) { }

		protected override void Parse( BinaryReader reader )
		{
			if ( Context.Data is null || reader.GetLength() < 16 )
				return;

			Id = reader.ReadInt32();
			reader.Skip<short>(); // ushort flags
			var gameLumpVersion = reader.ReadUInt16(); // ushort version
			var offset = reader.ReadInt32();
			var length = reader.ReadInt32();
			if ( offset < 0 || length <= 0 || offset > Context.Data.Length || length > (Context.Data.Length - offset) )
			{
				Log.Warning( $"[BSP] Skipping invalid game lump {Id}: offset={offset}, length={length}." );
				return;
			}

			// offset is based on full file start, aka raw initial data
			var gameLumpData = Context.Data.Take( new Range( offset, offset + length ) ).ToArray();
			try
			{
				switch ( (GameLumpType)Id )
				{
					case GameLumpType.StaticPropLump:
						_ = new StaticPropLump( Context, gameLumpData, gameLumpVersion );
						break;
				}
			}
			catch ( Exception ex )
			{
				Log.Error( $"Failed decompiling static prop game lump! {ex}" );
			}
		}

		private enum GameLumpType
		{
			StaticPropLump = 1936749168,
			DetailPropLump = 1685090928
		}
	}
}
