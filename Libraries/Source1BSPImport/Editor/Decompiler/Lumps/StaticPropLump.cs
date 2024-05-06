namespace BspImport.Decompiler.Lumps;

public class StaticPropLump : BaseLump
{
	private int DictEntryCount { get; set; }
	private Dictionary<int, string>? Names { get; set; }

	public StaticPropLump( ImportContext context, byte[] data, int version = 0 ) : base( context, data, version ) { }

	protected override void Parse( BinaryReader reader )
	{
		// parse static prop names (model names)
		DictEntryCount = reader.ReadInt32();
		Names = new();

		for ( int i = 0; i < DictEntryCount; i++ )
		{
			var size = Marshal.SizeOf<StaticPropNameEntry>();
			var sReader = new StructReader<StaticPropNameEntry>();
			var name = sReader.Read( reader.ReadBytes( size ) );

			var entry = new string( name.Name ).Trim( '\0' );

			Names.TryAdd( i, entry );
		}

		// we don't care about leaf entries
		var leafs = reader.ReadInt32(); // leaf entry count
		reader.Skip<ushort>( leafs ); // leaf entries

		// read static prop entries
		var entries = reader.ReadInt32();

		// no static props, don't bother
		if ( entries <= 0 )
			return;

		// size per static prop
		var propLength = reader.GetLength() / entries;

		for ( int i = 0; i < entries; i++ )
		{
			var sprp = reader.Split( propLength );

			var origin = sprp.ReadVector3();
			var angles = sprp.ReadVector3();

			var propType = sprp.ReadUInt16();

			var prop = new LumpEntity();
			prop.SetClassName( "prop_static" );
			prop.SetPosition( origin );
			prop.SetAngles( new Angles( angles ) );
			if ( Names.TryGetValue( propType, out var model ) )
			{
				prop.SetModel( Names[propType] );
			}


			// bit dirty but we only throw props into the entity lump here
			Context.Entities = Context.Entities?.Append( prop ).ToArray();
		}

		Log.Info( $"STATIC PROPS: {entries}" );
	}

	// helper for getting the dict entries
	private struct StaticPropNameEntry
	{
		[MarshalAs( UnmanagedType.ByValArray, SizeConst = 128 )]
		public char[] Name;

		public StaticPropNameEntry()
		{
			Name = new char[128];
		}
	}
}
