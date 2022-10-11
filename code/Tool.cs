using BspImport.Builder;
using BspImport.Decompiler;
using System.Runtime.CompilerServices;
using System.Threading;
using System.Threading.Tasks;

namespace BspImport;

public static class Tool
{
	[Menu( "Hammer", "Bsp Import/Import Map...", "map" )]
	public static void OpenLoadMenu()
	{
		var file = GetFileFromDialog( "Open a bsp file.", "*.bsp" );
		Log.Info( $"---------------" );
		Log.Info( $"LOADING BSP: {file}" );

		if ( file is null )
			return;

		Context = new DecompilerContext();

		// decompile in parallel, also prepares worldspawn geometry
		var dTask = new Task( () => Decompile( file ) );
		dTask.Start();
	}

	private static void Decompile( string file )
	{
		if ( Context is null )
			return;

		var decompiler = new MapDecompiler( Context );
		decompiler.Decompile( file );

		var builder = new MapBuilder( Context );
		builder.PrepareWorldSpawn();
	}

	public static DecompilerContext? Context { get; set; }

	private static RealTimeSince TimeSinceDecompileChecked = 0;

	[Event.Frame]
	public static void Tick()
	{
		// check once a second
		if ( !(TimeSinceDecompileChecked >= 1.0f) || Context is null )
			return;

		// reset timer
		TimeSinceDecompileChecked = 0;

		// check decompile status
		if ( Context.Decompiling || !Context.Decompiled || !Context.PreparedWorldSpawn )
			return;

		Log.Info( $"Finished decompile found, building..." );

		var builder = new MapBuilder( Context );
		builder.Build();

		// reset decompile status
		Context.Decompiling = true;
		Context.Decompiled = false;
	}

	private static string? GetFileFromDialog( string title = "Open File", string filter = "*.*" )
	{
		var file = new FileDialog( null );
		file.Title = title;
		file.SetNameFilter( filter );
		file.SetFindFile();

		if ( file.Execute() )
		{
			return file.SelectedFile;
		}
		return null;
	}
}
