namespace BspImport;

public class ImportSettings
{
	/// <summary>
	/// Controls the maximum number of faces per MeshComponent. This is necessary because editor performance quickly degrades with only a couple hundred faces per Component.
	/// </summary>
	[Range( 32, 1024 )]
	[Step( 32 )]
	public int ChunkSize { get; set; } = 256;

	/// <summary>
	/// Include static Prop models as GameObjects.
	/// </summary>
	public bool ImportStaticProps { get; set; } = true;

	/// <summary>
	/// Include Brush Entities as GameObjects
	/// </summary>
	public bool ImportBrushEntities { get; set; } = true;

	/// <summary>
	/// Include Misc Entities (Lights, Spawns, etc) as GameObjects.
	/// </summary>
	//public bool ImportEntities { get; set; } = true;

	/// <summary>
	/// Include Tool Materials (toolsskybox, trigger, etc) in the world mesh.
	/// </summary>
	public bool ImportToolMaterials { get; set; } = false;

	/// <summary>
	/// Include Displacement Meshes in the world mesh.
	/// </summary>
	public bool ImportDisplacements { get; set; } = true;
}
