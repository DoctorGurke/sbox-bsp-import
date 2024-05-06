namespace BspImport.Builder;

public class PolygonMeshBuilder
{
	public List<Vector3> Vertices = new();

	// adds a vert, returns the index
	public int AddVertex( Vector3 vertex )
	{
		if ( Vertices.Contains( vertex ) )
			return Vertices.IndexOf( vertex );

		Vertices.Add( vertex );
		return Vertices.Count - 1;
	}
}
