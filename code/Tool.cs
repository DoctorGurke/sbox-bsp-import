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

		var context = new DecompilerContext();

		var decompiler = new MapDecompiler( context );
		decompiler.Decompile( file );

		var builder = new MapBuilder( context );
		builder.Build();
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
