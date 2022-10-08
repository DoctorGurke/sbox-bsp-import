using BspImport.Decompiler;

namespace BspImport.Inspector;

public class MapInspector : Window
{
	public MapInspector( string file )
	{
		Title = "Inspector";
		Size = new( 1200, 800 );

		//Canvas = BuildContentWidget();

		//var decompiler = new GmodMapDecompiler( file );

		//Show();
	}
}
