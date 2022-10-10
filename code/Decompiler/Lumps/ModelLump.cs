namespace BspImport.Decompiler.Lumps;

public class ModelLump : BaseLump
{
	public ModelLump( DecompilerContext context, IEnumerable<byte> data, int version = 0 ) : base( context, data, version ) { }

	protected override void Parse( ByteParser data )
	{
		var list = new List<MapModel>();

		var models = data.BufferCapacity / 48;
		for ( int i = 0; i < models; i++ )
		{
			data.Skip<Vector3>(); // mins
			data.Skip<Vector3>(); // maxs
			var origin = data.Read<Vector3>();
			data.Skip<int>(); // headnode
			int firstface = data.Read<int>();
			int numfaces = data.Read<int>();

			var model = new MapModel( origin, firstface, numfaces );
			list.Add( model );
		}

		Log.Info( $"MODELS: {list.Count()}" );

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
