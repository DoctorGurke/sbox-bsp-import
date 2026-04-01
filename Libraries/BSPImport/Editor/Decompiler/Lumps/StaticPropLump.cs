using BspImport.Decompiler.Formats;

namespace BspImport.Decompiler.Lumps;

public class StaticPropLump : BaseLump
{
	private const int NameEntrySize = 128;

	private int DictEntryCount { get; set; }
	private Dictionary<int, string> Names { get; set; } = new();

	public StaticPropLump( ImportContext context, byte[] data, int version = 0 ) : base( context, data, version ) { }

	protected override void Parse( BinaryReader reader )
	{
		if ( reader.GetLength() < sizeof( int ) )
			return;

		DictEntryCount = reader.ReadInt32();
		if ( DictEntryCount < 0 )
			return;

		Names = new Dictionary<int, string>( DictEntryCount );

		for ( int i = 0; i < DictEntryCount; i++ )
		{
			if ( reader.GetLength() < NameEntrySize )
			{
				Log.Warning( $"[BSP] Static prop dictionary truncated at entry {i}/{DictEntryCount}." );
				return;
			}

			string entry = Encoding.ASCII.GetString( reader.ReadBytes( NameEntrySize ) ).TrimEnd( '\0' );
			Names[i] = entry;
		}

		if ( reader.GetLength() < sizeof( int ) )
			return;

		int leafRefCount = reader.ReadInt32();
		if ( leafRefCount < 0 )
			return;

		long leafRefBytes = (long)leafRefCount * sizeof( ushort );
		if ( reader.GetLength() < leafRefBytes )
		{
			Log.Warning( $"[BSP] Static prop leaf refs truncated: expected {leafRefCount} entries." );
			return;
		}

		reader.Skip<ushort>( leafRefCount );

		if ( reader.GetLength() < sizeof( int ) )
			return;

		int entryCount = reader.ReadInt32();
		if ( entryCount <= 0 )
			return;

		var structReaders = Context.FormatDescriptor.GetStructReaders( Context.BspVersion );
		var props = new List<LumpEntity>( entryCount );

		for ( int i = 0; i < entryCount; i++ )
		{
			StaticPropInstance staticProp;
			try
			{
				staticProp = structReaders.ReadStaticProp( reader, Context.FormatDescriptor, Version );
			}
			catch ( Exception ex ) when ( ex is ArgumentException or EndOfStreamException or InvalidOperationException )
			{
				Log.Warning( $"[BSP] Static prop entries truncated at entry {i}/{entryCount}: {ex.Message}" );
				break;
			}

			var prop = new LumpEntity();
			prop.SetClassName( "prop_static" );
			prop.SetPosition( staticProp.Origin );
			prop.SetAngles( new Angles( staticProp.Angles ) );

			if ( Names.TryGetValue( staticProp.PropType, out var model ) )
				prop.SetModel( model );

			props.Add( prop );
		}

		if ( props.Count == 0 )
			return;

		Context.Entities = Context.Entities is { Length: > 0 }
			? Context.Entities.Concat( props ).ToArray()
			: props.ToArray();

		//Log.Info( $"STATIC PROPS: {entryCount}" );
	}
}
