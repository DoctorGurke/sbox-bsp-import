using System.Runtime.InteropServices;

namespace BspImport.Decompiler.Lumps;

public class StaticPropLump : BaseLump
{
	private int DictEntryCount { get; set; }
	private Dictionary<int, string>? Names { get; set; }

	public StaticPropLump( DecompilerContext context, byte[] data, int version = 0 ) : base( context, data, version ) { }

	protected override void Parse( ByteParser data )
	{
		// parse static prop names (model names)
		DictEntryCount = data.Read<int>();
		Names = new();

		for ( int i = 0; i < DictEntryCount; i++ )
		{
			var name = data.Read<StaticPropNameEntry>();
			var entry = new string( name.Name ).Trim( '\0' );

			Names.TryAdd( i, entry );
		}

		// we don't care about leaf entries
		var leafs = data.Read<int>(); // leaf entry count
		data.Skip<ushort>( leafs ); // leaf entries

		// read static prop entries
		var entries = data.Read<int>();

		// no static props, don't bother
		if ( entries <= 0 )
			return;

		// size per static prop
		var propLength = data.BufferCapacity / entries;

		for ( int i = 0; i < entries; i++ )
		{
			var sprp = new ByteParser( data.ReadBytes( propLength ) );

			var origin = sprp.Read<Vector3>();
			var angles = sprp.Read<Angles>();

			var propType = sprp.Read<ushort>();

			var prop = new LumpEntity();
			prop.SetClassName( "prop_static" );
			prop.SetPosition( origin );
			prop.SetAngles( angles );
			prop.SetModel( Names[propType] );

			// bit dirty but we only throw props into the entity lump once
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
