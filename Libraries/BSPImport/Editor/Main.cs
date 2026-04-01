namespace BspImport;

public static class Main
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
		ps.AddProperty( settings, x => x.FilePath );
		ps.AddProperty( settings, x => x.ChunkSize );
		ps.AddProperty( settings, x => x.LoadMaterials );
		ps.AddProperty( settings, x => x.LoadModels );
		ps.AddProperty( settings, x => x.ImportEntities );
		ps.AddProperty( settings, x => x.ImportToolMaterials );
		ps.AddProperty( settings, x => x.ImportWorldGeometry );

		canvas.Layout.Add( ps );

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

	static Vector3[] positions = new[]
	{
		new Vector3(784f, 1216f, 224.00002f),
		new Vector3(784f, 1216f, 228f),
		new Vector3(784f, 1216f, 232f),
		new Vector3(784f, 1216f, 236f),
		new Vector3(784f, 1216f, 240f),
		new Vector3(812.0001f, 1216.0002f, 224.00003f),
		new Vector3(812.0001f, 1216.0002f, 228.00002f),
		new Vector3(812.0001f, 1216.0002f, 232.00002f),
		new Vector3(812.0001f, 1216.0002f, 236.00003f),
		new Vector3(812.0001f, 1216.0002f, 240.00002f),
		new Vector3(840f, 1216f, 224f),
		new Vector3(840f, 1216f, 228.00002f),
		new Vector3(840f, 1216f, 232.00002f),
		new Vector3(840f, 1216f, 236f),
		new Vector3(840f, 1216f, 240.00002f),
		new Vector3(868.00006f, 1216f, 224.00002f),
		new Vector3(868.00006f, 1216f, 228.00002f),
		new Vector3(868.00006f, 1216f, 232.00002f),
		new Vector3(868.00006f, 1216f, 236f),
		new Vector3(868.00006f, 1216f, 240.00002f),
		new Vector3(896f, 1216f, 224f),
		new Vector3(896f, 1216f, 228f),
		new Vector3(896f, 1216f, 232f),
		new Vector3(896f, 1216f, 236f),
		new Vector3(896f, 1216f, 240f),
	};

	static int[][] quads = new int[][]
	{
		[0, 1, 5, 6],
		[1, 2, 6, 7],
		[2, 3, 7, 8],
		[3, 4, 8, 9],
		[5, 6, 10, 11],
		[6, 7, 11, 12],
		[7, 8, 12, 13],
		[8, 9, 13, 14],
		[10, 11, 15, 16],
		[11, 12, 16, 17],
		[12, 13, 17, 18],
		[13, 14, 18, 19],
		[15, 16, 20, 21],
		[16, 17, 21, 22],
		[17, 18, 22, 23],
		[18, 19, 23, 24]
	};

	//[Menu( "Editor", "Issue/Test Mesh Hang", "settings" )]
	public static void TestIssue()
	{
		var mesh = new PolygonMesh();
		var hVerts = mesh.AddVertices( positions );

		foreach ( var quad in quads )
		{
			var a = quad[0];
			var b = quad[1];
			var c = quad[2];
			var d = quad[3];

			var faceVerts = new[] { hVerts[a], hVerts[c], hVerts[d], hVerts[b] };
			var hFace = mesh.AddFace( faceVerts );
		}

		Log.Info( "testing..." );

		// hang
		mesh.Rebuild();

		Log.Info( "passed" );
	}
}
