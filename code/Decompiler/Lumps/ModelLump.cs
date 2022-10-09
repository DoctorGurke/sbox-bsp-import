namespace BspImport.Decompiler.Lumps;

public class ModelLump : BaseLump
{
	public ModelLump( DecompilerContext context, IEnumerable<byte> data, int version = 0 ) : base( context, data, version )
	{
		var parser = new ByteParser( data );

		var list = new List<MapModel>();

		for ( int i = 0; i < data.Count() / 48; i++ )
		{
			var mins = parser.Read<Vector3>();
			var maxs = parser.Read<Vector3>();
			var origin = parser.Read<Vector3>();
			int headnode = parser.Read<int>();
			int firstface = parser.Read<int>();
			int numfaces = parser.Read<int>();

			var model = new MapModel( mins, maxs, origin, headnode, firstface, numfaces );
			list.Add( model );
		}

		Context.Models = list;
	}
}
