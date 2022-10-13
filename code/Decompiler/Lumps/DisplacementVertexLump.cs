namespace BspImport.Decompiler.Lumps;

public class DisplacementVertexLump : BaseLump
{
	public DisplacementVertexLump( ImportContext context, byte[] data, int version = 0 ) : base( context, data, version ) { }

	protected override void Parse( BinaryReader reader )
	{
		var vertCount = reader.GetLength() / Marshal.SizeOf<DisplacementVertex>();

		var verts = new DisplacementVertex[vertCount];

		for ( int i = 0; i < vertCount; i++ )
		{
			verts[i] = new DisplacementVertex( reader.ReadVector3(), reader.ReadSingle(), reader.ReadSingle() );
		}

		Log.Info( $"DISPLACEMENT VERTICES: {verts.Length}" );

		Context.Geometry.DisplacementVertices = verts;
	}
}

public struct DisplacementVertex
{
	public Vector3 Position;
	public float Distance;
	public float Alpha;

	public DisplacementVertex( Vector3 position, float distance, float alpha )
	{
		Position = position;
		Distance = distance;
		Alpha = alpha;
	}
}
