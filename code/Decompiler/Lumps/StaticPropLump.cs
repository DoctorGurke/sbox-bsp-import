using BspImport.Extensions;
using System.Net.Http.Headers;
using System.Runtime.InteropServices;

namespace BspImport.Decompiler.Lumps;

public class StaticPropLump : BaseLump
{
	private int DictEntryCount { get; set; }
	private Dictionary<int, string>? Names { get; set; }

	public StaticPropLump( DecompilerContext context, byte[] data, int version = 0 ) : base( context, data, version ) { }

	protected override void Parse( BinaryReader reader, int capacity )
	{
		// parse static prop names (model names)
		DictEntryCount = reader.ReadInt32();
		Names = new();

		for ( int i = 0; i < DictEntryCount; i++ )
		{
			var size = Marshal.SizeOf( typeof( StaticPropNameEntry ) );
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

		Log.Info( $"static props: buffer: {reader.GetLength()} entries: {entries}" );

		for ( int i = 0; i < entries; i++ )
		{
			Log.Info( $"proplength: {propLength} reader: {reader.GetLength()}" );
			var sprp = reader.Split( propLength );

			Log.Info( $"split: {sprp.GetLength()}" );

			var origin = sprp.ReadVector3();
			var angles = sprp.ReadAngles();

			var propType = sprp.ReadUInt16();

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
