namespace BspImport;

public class ImportSettings
{
	/// <summary>
	/// Controls the maximum number of faces per MeshComponent. This is necessary because editor performance quickly degrades with only a couple hundred faces per Component.
	/// </summary>
	[Range( 32, 1024 )]
	[Step( 32 )]
	public int ChunkSize { get; set; } = 256;
}
