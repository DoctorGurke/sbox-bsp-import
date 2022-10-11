using BspImport.Builder;
using BspImport.Decompiler;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;

namespace BspImport;

public static class Tool
{
	[Menu( "Hammer", "Bsp Import/Import Map...", "map" )]
	public static void OpenLoadMenu()
	{
		var file = GetFileFromDialog( "Open a bsp file.", "*.bsp" );
		Log.Info( $"### Loading bsp: {file}" );

		if ( file is null )
			return;

		var data = File.ReadAllBytes( file );

		Context = new DecompilerContext( data );

		// decompile in parallel, also prepares worldspawn geometry
		Context.DecompileTask = new Task( () => Decompile() );
		Context.DecompileTask.Start();
	}

	private static void Decompile()
	{
		// run in parallel
		ThreadSafe.AssertIsNotMainThread();

		if ( Context is null )
			return;

		var decompiler = new MapDecompiler( Context );
		decompiler.Decompile();
	}

	public static DecompilerContext? Context { get; set; }

	[Event.Frame] // main thread
	public static void CheckDecompile()
	{
		// main thread
		ThreadSafe.AssertIsMainThread();

		// check Context state
		if ( Context is null || Context.DecompileTask is null || Context.DecompileTask.Status != TaskStatus.RanToCompletion )
			return;

		// reset decompile task
		Context.DecompileTask = null;

		Log.Info( $"Decompiled Context found, Caching..." );

		// cache materials, block main thread
		var builder = new MapBuilder( Context );
		builder.CacheMaterials();

		// cache meshes in parallel
		Context.CacheTask = new Task( () => builder.CachePolygonMeshes() );
		Context.CacheTask.Start();
	}

	[Event.Frame]
	public static void CheckCached()
	{
		// main thread
		ThreadSafe.AssertIsMainThread();

		// check Context state
		if ( Context is null || Context.CacheTask is null || Context.CacheTask.Status != TaskStatus.RanToCompletion )
			return;

		// reset cache task
		Context.CacheTask = null;

		Log.Info( $"Cached Context found, Building..." );

		var builder = new MapBuilder( Context );
		builder.Build();

		Log.Info( $"Done building." );
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
