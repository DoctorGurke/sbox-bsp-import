using System.Threading.Tasks;

namespace BspImport.Decompiler;

[MapDecompiler( "Default" )]
public partial class MapDecompiler
{
	protected DecompilerContext Context { get; set; }

	public MapDecompiler( DecompilerContext context )
	{
		Context = context;
	}

	public async virtual void Decompile()
	{
		Log.Info( $"Decompiling Context..." );

		var reader = new BinaryReader( new MemoryStream( Context.Data ) );

		// parse bsp header
		var ident = reader.ReadInt32();
		var mapversion = reader.ReadInt32();

		var tasks = new List<Task>();

		// 64 lump headers
		for ( int i = 0; i < 64; i++ )
		{
			var offset = reader.ReadInt32();
			var length = reader.ReadInt32();
			var version = reader.ReadInt32();
			reader.Skip( 4 ); // fourCC

			// don't bother
			if ( !Enum.IsDefined( typeof( LumpType ), i ) )
			{
				continue;
			}

			// prepare lump data
			byte[] lumpData = new byte[length];
			Array.Copy( Context.Data, offset, lumpData, 0, length );

			var lump = ParseLump( i, lumpData, version );

			if ( lump is null )
				continue;

			Context.Lumps[i] = lump;

			var task = new Task( () => lump.Parse() );
			task.Start();
			tasks.Add( task );
		}

		// wait for all lumps to finish compiling
		await Task.WhenAll( tasks.ToArray() );

		var revision = reader.ReadInt32();

		Log.Info( $"Finished Decompiling: [ident: {ident} version: {mapversion} revision: {revision}]" );

		Context.Decompiled = true;
	}
}
