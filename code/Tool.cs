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

		var context = new ImportContext( data );
		context.Decompile();
		context.Build();
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
