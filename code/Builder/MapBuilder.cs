using BspImport.Decompiler;
using Tools.MapDoc;
using Tools.MapEditor;

namespace BspImport.Builder;

public class MapBuilder
{

	protected DecompilerContext Context { get; set; }

	public MapBuilder( DecompilerContext context )
	{
		Context = context;
	}

	public void Build()
	{
		BuildEntities();

		BuildStaticProps();

		BuildGeometry();
	}

	protected virtual void BuildEntities()
	{
		if ( Context.Entities is null )
			return;

		foreach ( var ent in Context.Entities )
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

	protected virtual void BuildStaticProps()
	{

	}

	protected virtual void BuildGeometry()
	{

	}
}
