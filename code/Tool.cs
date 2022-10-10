using BspImport.Builder;
using BspImport.Decompiler;

namespace BspImport;

public static class Tool
{
	[Menu( "Hammer", "Bsp Import/Load Map...", "map" )]
	public static void OpenLoadMenu()
	{
		var file = GetFileFromDialog( "Open a bsp file.", "*.bsp" );
		Log.Info( $"---------------" );
		Log.Info( $"LOADING BSP: {file}" );

		if ( file is null )
			return;

		var context = new DecompilerContext();

		//var decompiler = new GmodMapDecompiler( file );
		var decompiler = new MapDecompiler( context );
		decompiler.Decompile( file );

		var builder = new MapBuilder( context );
		builder.Build();
	}

	//[Menu( "Hammer", "Bsp Import/Inspect Map...", "construction" )]
	//public static void OpenInspectMenu()
	//{
	//	var file = GetFileFromDialog( "Open a bsp file.", "*.bsp" );
	//	Log.Info( file );

	//	if ( file is null )
	//		return;

	//	_ = new MapInspector( file );
	//}

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
