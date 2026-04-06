namespace BspImport.Tools;

public static class ImportMenu
{
	/// <summary>
	/// Main entry point for the tool. Prompt user to import a bsp file.
	/// </summary>
	[Menu( "Editor", "BSP Import/Import Map...", "map" )]
	public static void OpenLoadMenu()
	{
		var window = new Window( null );
		window.WindowTitle = "BSP Import Settings";

		window.Canvas = new Widget( window );
		var canvas = window.Canvas;

		canvas.Layout = Layout.Column();
		canvas.Layout.Margin = 16;
		canvas.Layout.Spacing = 4;

		var newSettings = new ImportSettings();

		var cookieString = "bsp-import.last-imported-bsp";
		var settings = Game.Cookies.Get( cookieString, newSettings );

		var ps = new ControlSheet();
		ps.AddObject( settings.GetSerialized() );

		canvas.Layout.Add( ps );
		canvas.Layout.AddStretchCell();

		var btn = new Button( "Import", canvas );
		btn.MouseClick += () =>
		{
			Game.Cookies.Set<ImportSettings>( cookieString, settings );
			DecompileAndImport( settings );
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
	private static void DecompileAndImport( ImportSettings settings )
	{
		var data = Editor.FileSystem.Content.ReadAllBytes( settings.FilePath );
		var name = Path.GetFileName( settings.FilePath );
		var context = new ImportContext( name, data.ToArray(), settings );
		context.Decompile();
		context.Build();
	}
}

