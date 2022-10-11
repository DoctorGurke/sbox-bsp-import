namespace BspImport.Decompiler.Lumps;

public class VertexLump : BaseLump
{
	public VertexLump( DecompilerContext context, byte[] data, int version = 0 ) : base( context, data, version ) { }

	protected override void Parse( ByteParser data )
	{
		var bReader = new BinaryReader( new MemoryStream( data ) );

		var vertexCount = data.BufferCapacity / (sizeof( float ) * 3); // how many vec3s are in the buffer

		var vertices = new Vector3[vertexCount];

		for ( int i = 0; i < vertexCount; i++ )
		{
			vertices[i] = new Vector3( bReader.ReadSingle(), bReader.ReadSingle(), bReader.ReadSingle() );
		}

		Log.Info( $"VERTICES: {vertices.Length}" );

		Context.MapGeometry.VertexPositions = vertices;
	}
}
