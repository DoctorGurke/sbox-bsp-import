using System.Linq;
using System.Runtime.InteropServices;

namespace BspImport.Decompiler.Lumps;

public class StaticPropLump : BaseLump
{
	private int DictEntryCount { get; set; }
	private Dictionary<int, string> Names { get; set; }

	public StaticPropLump( DecompilerContext context, IEnumerable<byte> data, int version = 0 ) : base( context, data, version )
	{
		Log.Info( $"static prop lump" );
		var parser = new ByteParser( data );

		// parse static prop names (model names)
		DictEntryCount = parser.Read<int>();
		Names = new();

		Log.Info( $"name entries: {DictEntryCount}" );
		for ( int i = 0; i < DictEntryCount; i++ )
		{
			var name = parser.Read<StaticPropNameEntry>();
			var entry = new string( name.Name ).Trim( '\0' );

			Log.Info( $"{entry}" );
			Names.TryAdd( i, entry );
		}

		// we don't care about leaf entries
		var leafs = parser.Read<int>(); // leaf entry count
		parser.Skip<ushort>( leafs ); // leaf entries

		// read static prop entries
		var entries = parser.Read<int>();
		Log.Info( $"static props: {entries}" );

		// size per static prop
		var sizeper = parser.BufferCapacity / entries;

		for ( int i = 0; i < entries; i++ )
		{
			var sprp = new ByteParser( parser.ReadBytes( sizeper ) );

			var origin = sprp.Read<Vector3>();
			var angles = sprp.Read<Angles>();

			var proptype = sprp.Read<ushort>();

			Log.Info( $"### PROP STATIC ###" );
			Log.Info( $"origin: {origin}" );
			Log.Info( $"angles: {angles}" );
			Log.Info( $"index: {proptype} :: {Names[proptype]}" );

			var prop = new LumpEntity();
			prop.SetClassName( "prop_static" );
			prop.SetPosition( origin );
			prop.SetAngles( angles );
			prop.SetModel( Names[proptype] );

			Context.Entities = Context.Entities?.Append( prop );
		}

	}

	// helper for getting the dict entries
	private struct StaticPropNameEntry
	{
		[MarshalAs( UnmanagedType.ByValArray, SizeConst = 128 )]
		public char[] Name;
	}
}
