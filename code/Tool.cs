namespace BspImport;

public static class Tool
{
	[Menu( "Hammer", "Bsp Import/Load Map...", "map" )]
	public static void OpenLoadMenu()
	{
		Log.Info( GetFileFromDialog( "Open a bsp file.", "*.bsp" ) );
	}

	[Menu( "Hammer", "Bsp Import/Inspect Map...", "construction" )]
	public static void OpenInspectMenu()
	{
		Log.Info( GetFileFromDialog( "Open a bsp file.", "*.bsp" ) );
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
