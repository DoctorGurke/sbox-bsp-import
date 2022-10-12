namespace BspImport.Decompiler.Lumps;

public class ModelLump : BaseLump
{
	public ModelLump( ImportContext context, byte[] data, int version = 0 ) : base( context, data, version ) { }

	protected override void Parse( BinaryReader reader )
	{
		var modelCount = reader.GetLength() / 48;

		var models = new MapModel[modelCount];

		for ( int i = 0; i < modelCount; i++ )
		{
			reader.Skip<Vector3>(); // vec3 mins
			reader.Skip<Vector3>(); // vec3 maxs
			var origin = reader.ReadVector3();
			reader.Skip<int>(); // int headnode
			int firstFace = reader.ReadInt32();
			int numFaces = reader.ReadInt32();

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
