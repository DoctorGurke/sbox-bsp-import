namespace BspImport.Decompiler.Lumps;

public class ModelLump : BaseLump
{
	public ModelLump( DecompilerContext context, byte[] data, int version = 0 ) : base( context, data, version ) { }

	protected override void Parse( ByteParser data )
	{
		var modelCount = data.BufferCapacity / 48;

		var models = new MapModel[modelCount];

		for ( int i = 0; i < modelCount; i++ )
		{
			data.Skip<Vector3>(); // mins
			data.Skip<Vector3>(); // maxs
			var origin = data.Read<Vector3>();
			data.Skip<int>(); // headnode
			int firstFace = data.Read<int>();
			int numFaces = data.Read<int>();

			var model = new MapModel( origin, firstFace, numFaces );
			models[i] = model;
		}

		Log.Info( $"MODELS: {models.Length}" );

		Context.Models = models;
	}
}

public struct MapModel
{
	public Vector3 Origin;
	public int FirstFace;
	public int FaceCount;

	public MapModel( Vector3 origin, int firstFace, int faceCount )
	{
		Origin = origin;
		FirstFace = firstFace;
		FaceCount = faceCount;
	}
}
