using BspImport.Extensions;

namespace BspImport.Decompiler.Lumps;

public class SurfaceEdgeLump : BaseLump
{
	public SurfaceEdgeLump( ImportContext context, byte[] data, int version = 0 ) : base( context, data, version ) { }

	protected override void Parse( BinaryReader reader )
	{
		var surfEdgeCount = reader.GetLength() / sizeof( int );

		var surfEdges = new int[surfEdgeCount];

		for ( int i = 0; i < surfEdgeCount; i++ )
		{
			surfEdges[i] = reader.ReadInt32();
		}

		Log.Info( $"SURFACE EDGES: {surfEdges.Length}" );

		Context.Geometry.SurfaceEdges = surfEdges;
	}
}
