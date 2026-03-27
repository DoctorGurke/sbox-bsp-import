namespace BspImport;

public class ImportSettings
{
	[Property, FilePath( Extension = "bsp" )]
	public string FilePath { get; set; } = string.Empty;

	/// <summary>
	/// Controls the maximum number of faces per MeshComponent. This is necessary because editor performance quickly degrades with only a couple hundred faces per Component.
	/// </summary>
	[Range( 32, 1024 )]
	[Step( 32 )]
	[Property]
	public int ChunkSize { get; set; } = 256;

	/// <summary>
	/// Load and Apply materials to world geometry.
	/// </summary>
	[Property]
	public bool LoadMaterials { get; set; } = false;

	/// <summary>
	/// Load and spawn Model paths as Props. Includes static props.
	/// </summary>
	[Property]
	public bool LoadModels { get; set; } = false;

	/// <summary>
	/// Include Entities (Lights, Brush Entities, etc) as GameObjects.
	/// </summary>
	[Property]
	public bool ImportEntities { get; set; } = true;

	/// <summary>
	/// Include Tool Materials (toolsskybox, trigger, etc) in the world mesh.
	/// </summary>
	[Property]
	public bool ImportToolMaterials { get; set; } = false;

	/// <summary>
	/// Include Displacement Meshes in the world mesh.
	/// </summary>
	[Property]
	public bool ImportDisplacements { get; set; } = true;
}
