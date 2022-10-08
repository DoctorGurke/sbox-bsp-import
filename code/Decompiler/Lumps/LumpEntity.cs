using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BspImport.Decompiler.Lumps;

public class LumpEntity
{
	public List<KeyValuePair<string, string>> Data { get; set; } = new List<KeyValuePair<string, string>>();

	public string? GetValue( string key )
	{
		if ( !Data.Any() )
		{
			return null;
		}

		var find = Data.Where( x => x.Key == key );

		if ( !find.Any() )
		{
			return null;
		}

		var val = find.FirstOrDefault().Value;
		return val;
	}

	public string? ClassName => GetValue( "classname" );
	public Vector3 Position => Vector3.Parse( $"[{GetValue( "origin" ) ?? ""}]" );

	private Angles ConstructAngles()
	{
		var split = GetValue( "angles" )?.Split( ' ' );

		if ( split is null || split.Length != 3 )
			return Angles.Zero;

		return Angles.Parse( $"{split[0]},{split[1]},{split[2]}" );
	}

	public Angles Angles => ConstructAngles();
}
