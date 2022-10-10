namespace BspImport.Decompiler.Lumps;

public class SurfaceEdgeLump : BaseLump
{
	public SurfaceEdgeLump( DecompilerContext context, IEnumerable<byte> data, int version = 0 ) : base( context, data, version ) { }

	protected override void Parse( ByteParser data )
	{
		var surfedges = data.TryReadMultiple<int>();

		Log.Info( $"SURFACE EDGES: {surfedges.Count()}" );

		Context.MapGeometry.SurfaceEdges = surfedges;
	}
}
