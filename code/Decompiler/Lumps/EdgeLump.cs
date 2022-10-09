using System.Runtime.InteropServices;

namespace BspImport.Decompiler.Lumps;

public class EdgeLump : BaseLump
{
	public EdgeLump( DecompilerContext context, IEnumerable<byte> data, int version = 0 ) : base( context, data, version )
	{
		var parser = new ByteParser( data );
		var edges = parser.TryReadMultiple<EdgeIndices>();

		Log.Info( $"EDGES: {edges.Count()}" );

		Context.MapGeometry.EdgeIndices = edges;
	}
}

public struct EdgeIndices
{
	[MarshalAs( UnmanagedType.ByValArray, SizeConst = 2 )]
	public ushort[] Indices;
}
