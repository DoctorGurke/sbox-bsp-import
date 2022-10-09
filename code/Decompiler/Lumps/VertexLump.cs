namespace BspImport.Decompiler.Lumps;

public class VertexLump : BaseLump
{
	public VertexLump( DecompilerContext context, IEnumerable<byte> data, int version = 0 ) : base( context, data, version )
	{
		var parser = new ByteParser( data );
		var vertices = parser.TryReadMultiple<Vector3>();

		Log.Info( $"VERTICES: {vertices.Count()}" );

		Context.MapGeometry.VertexPositions = vertices;
	}
}
