using BspImport.Builder;
using BspImport.Decompiler.Formats;
using BspImport.Decompiler.Formats.Descriptors;

namespace BspImport;

public class ImportContext
{
	public ImportContext( string name, byte[] data, ImportSettings settings )
	{
		Name = name;
		Data = data;
		Settings = settings;

		Lumps = new BaseLump[64];
		Geometry = new();

		// Start as unknown; MapDecompiler will assign the correct descriptor
		// after reading the BSP header version (and optionally refining via entities).
		FormatDescriptor = UnknownBspFormatDescriptor.Instance;
	}

	/// <summary>
	/// Decompiles the data of the context.
	/// </summary>
	public void Decompile()
	{
		var decompiler = new MapDecompiler( this );
		decompiler.Decompile();
	}

	/// <summary>
	/// Construct the decompiled context into the scene.
	/// </summary>
	public void Build( GameObject? parent = null )
	{
		var builder = new MapBuilder( this );
		builder.Build( parent );
	}

	public string Name { get; private set; }
	public byte[] Data { get; private set; }
	public ImportSettings Settings { get; private set; }

	public BaseLump[] Lumps;

	/// <summary>
	/// The exact BSP version integer read from the file header.
	/// Stored separately from <see cref="FormatDescriptor"/> because one descriptor
	/// may support multiple versions and struct readers may differ between them.
	/// Set by <see cref="MapDecompiler"/> immediately after reading the header.
	/// </summary>
	public int BspVersion { get; set; }

	/// <summary>
	/// The detected BSP format descriptor for this file.
	/// Set by <see cref="MapDecompiler"/> immediately after reading the BSP header version,
	/// and optionally refined after the entity lump (lump 0) is parsed.
	/// Consumed by lumps that have format-specific binary layouts (e.g. FaceLump).
	/// </summary>
	public IBspFormatDescriptor FormatDescriptor { get; set; }

	// bsp tree structure
	public MapNode[]? Nodes;
	public MapLeaf[]? Leafs;
	public Plane[]? Planes;

	public Brush[]? Brushes { get; set; }
	public BrushSide[]? BrushSides { get; set; }

	public LumpEntity[]? Entities { get; set; }
	public MapModel[]? Models { get; set; }
	public GameLump[]? GameLumps { get; set; }
	public MapGeometry Geometry { get; private set; }
	public TexInfo[]? TexInfo { get; set; }
	public TexData[]? TexData { get; set; }
	public int[]? TexDataStringTable { get; set; }
	public TexDataStringData TexDataStringData { get; set; }

	public PolygonMesh[]? CachedPolygonMeshes { get; set; }
	public List<short> SkyboxAreas { get; set; } = new();

	/// <summary>
	/// Checks that the context has a complete geometry set available for building meshes.
	/// Returns the Geometry instance for convenience.
	/// </summary>
	public bool HasCompleteGeometry( out MapGeometry geo )
	{
		geo = Geometry;
		return geo is not null
			 && geo.IsValid();
	}
}
