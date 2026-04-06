using System;
using System.Collections.Generic;
using System.Text;

namespace BspImport.Tools.Data;

internal class InspectorSettings
{
	[Property, FilePath( Extension = "bsp" )]
	public string FilePath { get; set; } = string.Empty;
}
