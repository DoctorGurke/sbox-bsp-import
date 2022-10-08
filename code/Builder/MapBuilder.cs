using BspImport.Decompiler;
using Tools.MapDoc;
using Tools.MapEditor;

namespace BspImport.Builder;

public static class MapBuilder
{
	public static void Build()
	{
		BuildEntities();

		BuildStaticProps();

		BuildGeometry();
	}

	private static void BuildEntities()
	{
		if ( DecompilerContext.Entities is null )
			return;

		foreach ( var ent in DecompilerContext.Entities )
		{
			if ( ent.ClassName == "worldspawn" )
				continue;

			var mapent = new MapEntity( Hammer.ActiveMap );
			mapent.ClassName = ent.ClassName;
			mapent.Position = ent.Position;
			mapent.Angles = ent.Angles;

			foreach ( var kvp in ent.Data )
			{
				mapent.SetKeyValue( kvp.Key, kvp.Value );
			}
		}
	}

	private static void BuildStaticProps()
	{

	}

	private static void BuildGeometry()
	{

	}
}
