using Editor;

namespace BspImport;

public static class Main
{
	/// <summary>
	/// Main entry point for the tool. Prompt user to import a bsp file.
	/// </summary>
	[Menu( "Editor", "BSP Import/Import Map...", "map" )]
	public static void OpenLoadMenu()
	{
		// get bsp file path
		var file = GetFileFromDialog( "Open a bsp file.", "*.bsp" );
		Log.Info( $"### Loading bsp: {file}" );

		if ( file is null )
			return;

		// read bsp byte data, decompile into context, read and build
		var data = File.ReadAllBytes( file );
		var context = new ImportContext( data );
		context.Decompile();
		context.Build();

		//Log.Info( $"Thanks for using sbox-bsp-import by DoctorGurke to import outdated bsp maps from tf2 and gmod, anything else probably won't work right now." );
		//Log.Info( $"Issue reports: https://github.com/DoctorGurke/sbox-bsp-import/issues" );
	}

	//[Menu( "Hammer", "Bsp Import/Donate", "money" )]
	//public static void OpenDonate()
	//{
	//	Utility.OpenFolder( "https://paypal.me/DoctorGurke" );
	//	Log.Info( $"{Clipboard.Paste()}" );
	//	Utility.
	//}

	/// <summary>
	/// Gets a file path from a file explorer dialog.
	/// </summary>
	/// <param name="title">Title of the dialog window.</param>
	/// <param name="filter">File filter for dialog display.</param>
	/// <returns>File path of a singular user-selected file. Null if dialog was closed or failed.</returns>
	private static string? GetFileFromDialog( string title = "Open File", string filter = "*.*" )
	{
		var file = new FileDialog( null );
		file.Title = title;
		file.SetNameFilter( filter );
		file.SetFindFile();

		// user has selected file
		if ( file.Execute() )
		{
			return file.SelectedFile;
		}

		//// dialog was closed or failed, no file was selected.
		return null;
	}
}
