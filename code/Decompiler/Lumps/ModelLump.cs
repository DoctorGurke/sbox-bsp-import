namespace BspImport.Decompiler.Lumps;

public class ModelLump : BaseLump
{
	public ModelLump( DecompilerContext context, IEnumerable<byte> data, int version = 0 ) : base( context, data, version ) { }

	protected override void Parse( IEnumerable<byte> data )
	{
		var parser = new ByteParser( data );

		var list = new List<MapModel>();

		for ( int i = 0; i < data.Count() / 48; i++ )
		{
			parser.Skip<Vector3>();
			parser.Skip<Vector3>();
			var origin = parser.Read<Vector3>();
			parser.Skip<int>();
			int firstface = parser.Read<int>();
			int numfaces = parser.Read<int>();

			var model = new MapModel( origin, firstface, numfaces );
			list.Add( model );
		}

		Context.Models = list;
	}
}

public struct MapModel
{
	public Vector3 Origin;
	public int FirstFace;
	public int FaceCount;

	public MapModel( Vector3 origin, int firstface, int facecount )
	{
		Origin = origin;
		FirstFace = firstface;
		FaceCount = facecount;
	}
}
