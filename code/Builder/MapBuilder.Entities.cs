namespace BspImport.Builder;

public partial class MapBuilder
{
	/// <summary>
	/// Build entities parsed from entity lump and static props
	/// </summary>
	protected virtual void BuildEntities()
	{
		if ( Context.Entities is null )
			return;

		foreach ( var ent in Context.Entities )
		{
			// don't do shit with the worldspawn ent
			if ( ent.ClassName == "worldspawn" )
				continue;

			// brush entities
			if ( ent.Model is not null && ent.Model.StartsWith( '*' ) )
			{
				var modelIndex = int.Parse( ent.Model.TrimStart( '*' ) );
				var polyMesh = Context.CachedPolygonMeshes?[modelIndex];//ConstructModel( modelIndex, ent.Position, ent.Angles );

				if ( polyMesh is null )
				{
					continue;
				}

				// create entity
				var brushEntity = new MapEntity( Map );
				brushEntity.ClassName = ent.ClassName;
				brushEntity.Position = ent.Position;
				brushEntity.Angles = ent.Angles;

				// create and attach mesh, (tie mesh to entity)
				var mapMesh = new MapMesh( Map );
				mapMesh.ConstructFromPolygons( polyMesh );
				mapMesh.Parent = brushEntity;
				mapMesh.Position = ent.Position;
				mapMesh.Angles = ent.Angles;

				continue;
			}

			// regular entity
			var mapent = new MapEntity( Map );
			mapent.ClassName = ent.ClassName;
			mapent.Position = ent.Position;
			mapent.Angles = ent.Angles;
			mapent.Name = ent.GetValue( "targetname" ) ?? "";

			foreach ( var kvp in ent.Data )
			{
				mapent.SetKeyValue( kvp.Key, kvp.Value );
			}
		}
	}
}
