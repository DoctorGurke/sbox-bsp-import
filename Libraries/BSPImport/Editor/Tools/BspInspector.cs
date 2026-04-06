using BspImport.Tools.Data;
using SkiaSharp;
using System;
using System.Collections.Generic;
using System.Text;

namespace BspImport.Tools;

class BspInspector : Window
{

	//[Menu( "Editor", "BSP Import/Inspect BSP (debug)", "map" )]
	public static void OpenInspector()
	{
		var tool = new BspInspector();
		tool.Show();
	}

	private Widget DataWidget;

	public BspInspector()
	{
		WindowTitle = "BSP Inspector (debug)";

		Canvas = new Widget( this );
		Canvas.Layout = Layout.Row();
		Canvas.Layout.Margin = 16;
		Canvas.Layout.Spacing = 4;
		this.MaximumHeight = 700;
		this.MaximumWidth = 600;

		// left column, for settings
		var leftCol = new Widget( Canvas );
		leftCol.Layout = Layout.Column();
		leftCol.FixedWidth = 300;
		leftCol.Layout.Margin = 16;
		leftCol.Layout.Spacing = 4;
		leftCol.SetStyles( "background-color: #222222" );
		Canvas.Layout.Add( leftCol );
		Canvas.Layout.AddSeparator();

		// setup control
		var settings = new InspectorSettings();
		var settingsControl = new ControlSheet();
		var settingsSerialized = settings.GetSerialized();
		settingsSerialized.OnPropertyChanged += ( prop ) =>
		{
			if ( prop.Name == nameof( InspectorSettings.FilePath ) )
			{
				TryDecompile( settings );
			}

			RebuildDataWidget();
		};
		settingsControl.AddObject( settingsSerialized );

		leftCol.Layout.Add( settingsControl );
		leftCol.Layout.AddStretchCell();

		// right column, for info display
		DataWidget = new Widget( Canvas );
		DataWidget.Layout = Layout.Column();
		DataWidget.Layout.Margin = 16;
		DataWidget.Layout.Spacing = 4;
		DataWidget.Layout.AddStretchCell();
		DataWidget.SetStyles( "background-color: #222222" );
		Canvas.Layout.Add( DataWidget );

		RebuildDataWidget(); // does nothing until a valid context is loaded


		this.Center();
	}

	private void RebuildDataWidget()
	{
		DataWidget.DestroyChildren();

		if ( Context == null )
			return;

		var contextControl = new ControlSheet();
		var serialized = Context.Entities.GetSerialized();

		contextControl.AddObject( serialized );
		contextControl.IncludePropertyNames = true;
		DataWidget.Layout.Add( contextControl );
		DataWidget.Layout.AddStretchCell();
	}

	private ImportContext? Context { get; set; }

	private void TryDecompile( InspectorSettings settings )
	{
		var path = settings.FilePath;
		var data = Editor.FileSystem.Content.ReadAllBytes( path );
		var name = Path.GetFileName( path );

		Context = null;
		Context = new ImportContext( name, data.ToArray() );
		Context.Decompile();
	}

}
