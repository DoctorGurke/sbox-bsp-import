namespace BspImport.Decompiler.Lumps;

public class VertexLump : BaseLump
{
	public VertexLump( ImportContext context, byte[] data, int version = 0 ) : base( context, data, version ) { }

	protected override void Parse( BinaryReader reader )
	{
		var vertexCount = reader.GetLength() / Marshal.SizeOf<Vector3>(); // how many vec3s are in the buffer

		var vertices = new Vector3[vertexCount];

		for ( int i = 0; i < vertexCount; i++ )
		{
			vertices[i] = reader.ReadVector3();
		}

		Log.Info( $"VERTICES: {vertices.Length}" );

		Context.Geometry.Vertices = vertices;
	}
}
