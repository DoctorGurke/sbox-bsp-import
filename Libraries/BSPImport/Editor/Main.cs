namespace BspImport;


public class BspImportComponent : Component, Component.ExecuteInEditor
{
	[FilePath, Property]
	public string Path { get; set; } = string.Empty;

	[Property]
	public ImportSettings Settings { get; set; } = new ImportSettings();

	[Button( "Test", "map" )]
	public void Test()
	{
		// cleanup any children to get rid of previous import
		foreach ( var child in GameObject.Children )
		{
			child.Destroy();
		}

		Log.Info( $"Test load {Path}" );
		var data = Editor.FileSystem.Content.ReadAllBytes( Path );
		//var data = File.ReadAllBytes( Path );
		var context = new ImportContext( Path, data.ToArray(), Settings );
		context.Decompile();
		context.Build( GameObject );
	}
}

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


		var window = new Window( null );
		window.WindowTitle = "BSP Import Settings";

		window.Canvas = new Widget( window );
		var canvas = window.Canvas;

		canvas.Layout = Layout.Column();
		canvas.Layout.Margin = 16;
		canvas.Layout.Spacing = 4;

		var settings = new ImportSettings();

		var ps = new ControlSheet();

		ps.AddProperty( settings, x => x.ChunkSize );
		//ps.AddProperty(  );

		canvas.Layout.Add( ps );

		var btn = new Button( "Import", canvas );
		btn.MouseClick += () =>
		{
			DecompileAndImport( file, settings );
			window.Close();
		};
		canvas.Layout.Add( btn );

		window.FixedWidth = 500;
		window.Show();
		window.Center();

	}

	/// <summary>
	/// Read bsp byte data, decompile into ImportContext, parse and Build the map geometry and entities into the s&box scene.
	/// </summary>
	/// <param name="file"></param>
	private static void DecompileAndImport( string file, ImportSettings settings )
	{
		var data = File.ReadAllBytes( file );
		var name = Path.GetFileName( file );
		var context = new ImportContext( name, data, settings );
		context.Decompile();
		context.Build();


		Log.Info( "Imported Source 1 BSP File using bsp-import by DoctorGurke" );
		var repoURL = "https://github.com/DoctorGurke/sbox-bsp-import";
		Log.Info( $"Report bugs or contribute @{repoURL}" );
	}

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

		// dialog was closed or failed, no file was selected.
		return null;
	}
}
