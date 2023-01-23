namespace BspImport.Decompiler.Lumps;

public class EdgeLump : BaseLump
{
	public EdgeLump( ImportContext context, byte[] data, int version = 0 ) : base( context, data, version ) { }

	protected override void Parse( BinaryReader reader )
	{
		var edgeCount = reader.GetLength() / sizeof( int );

		var edges = new EdgeIndices[edgeCount];

		for ( int i = 0; i < edgeCount; i++ )
		{
			edges[i] = new EdgeIndices( reader.ReadUInt16(), reader.ReadUInt16() );
		}

		Log.Info( $"EDGES: {edges.Length}" );

		Context.Geometry.EdgeIndices = edges;
	}
}

public struct EdgeIndices
{
	public ushort[] Indices;

	public EdgeIndices( ushort index0, ushort index1 )
	{
		Indices = new ushort[2];
		Indices[0] = index0;
		Indices[1] = index1;
	}
}
