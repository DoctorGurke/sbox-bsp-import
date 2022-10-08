using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BspImport.Decompiler
{
	public class MapDecompilerAttribute : Attribute
	{
		public string Name { get; set; }
		public MapDecompilerAttribute( string name )
		{
			Name = name;
		}
	}
}
