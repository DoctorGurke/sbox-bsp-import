namespace BspImport;

public class ImportSettings
{
	[Property, FilePath( Extension = "bsp" )]
	public string FilePath { get; set; } = string.Empty;

	/// <summary>
	/// Include world geometry, including displacements.
	/// </summary>
	[Category( "Geometry" )]
	[Property]
	public bool ImportWorldGeometry { get; set; } = true;

	/// <summary>
	/// Controls the maximum number of faces per MeshComponent. This is necessary because editor performance quickly degrades with only a couple hundred faces per Component.
	/// </summary>
	[Range( 32, 1024 )]
	[Step( 32 )]
	[Category( "Geometry" )]
	[Property, ShowIf( nameof( ImportWorldGeometry ), true )]
	public int ChunkSize { get; set; } = 256;

	/// <summary>
	/// Load and Apply material paths to world geometry. NOTE: Does not create/port any assets.
	/// </summary>
	[Category( "Geometry" )]
	[Property, ShowIf( nameof( ImportWorldGeometry ), true )]
	public bool LoadMaterials { get; set; } = false;

	/// <summary>
	/// Include Entities (Lights, Brush Entities, etc) as GameObjects.
	/// </summary>
	[Category( "Entities" )]
	[Property]
	public bool ImportEntities { get; set; } = true;

	/// <summary>
	/// Load and spawn Model paths as Props, including static props. NOTE: Does not create/port any assets.
	/// </summary>
	[Category( "Entities" )]
	[Property, ShowIf( nameof( ImportEntities ), true )]
	public bool LoadModels { get; set; } = false;

	/// <summary>
	/// Cull 3D skybox Geometry and Models. 
	/// </summary>
	[Property]
	public bool CullSkybox { get; set; } = true;
}
