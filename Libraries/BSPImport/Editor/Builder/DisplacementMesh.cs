namespace BspImport.Builder;

internal class DisplacementMesh
{
	public DisplacementInfo Info { get; set; }

	public List<DisplacementVertex> Vertices { get; set; } = new();
}
