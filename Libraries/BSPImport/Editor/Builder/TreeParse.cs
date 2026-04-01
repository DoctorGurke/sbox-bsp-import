namespace BspImport.Builder;

public static class TreeParse
{
	public class TreeParseResult
	{
		public List<ushort> FaceIndices = new();
	}

	/// <summary>
	/// Get all unique Face indices from the BSP tree. Results represent render meshes, not brushes. Never brushes.
	/// </summary>
	/// <param name="context"></param>
	/// <returns></returns>
	public static TreeParseResult ParseTreeFaces( ImportContext context )
	{
		var result = new TreeParseResult();

		var faces = new HashSet<ushort>();
		ParseNodeFacesRecursively( context, 0, ref faces );

		result.FaceIndices = faces.ToList();

		return result;
	}

	private static void ParseNodeFacesRecursively( ImportContext context, int index, ref HashSet<ushort> faceIndices )
	{
		if ( context.Nodes is null )
			return;

		var node = context.Nodes[index];

		// contribute to faces collection
		for ( ushort i = 0; i < node.FaceCount; i++ )
		{
			ushort faceIndex = node.FirstFaceIndex;
			faceIndex += i;

			TryAddFace( context, faceIndex, ref faceIndices );
		}

		// gather faces from children
		for ( int i = 0; i < node.Children.Length; i++ )
		{
			var child = node.Children[i];

			// 0 = no child
			if ( child == 0 ) continue;

			// <0 = leaf, not node
			if ( child < 0 )
			{
				AddLeafFaces( context, -1 - child, ref faceIndices );
				continue;
			}

			// parse child node recursively
			ParseNodeFacesRecursively( context, child, ref faceIndices );
		}
	}

	private static void AddLeafFaces( ImportContext context, int index, ref HashSet<ushort> faceIndices )
	{
		if ( context.Leafs is null )
			return;

		if ( index >= context.Leafs.Length )
			return;

		var leaf = context.Leafs[index];

		// contribute to faces collection
		for ( ushort i = 0; i < leaf.FaceCount; i++ )
		{
			ushort leafFaceIndex = leaf.FirstFaceIndex;
			leafFaceIndex += i;

			context.Geometry.TryGetLeafFaceIndex( leafFaceIndex, out var faceIndex );

			TryAddFace( context, faceIndex, ref faceIndices );
		}
	}

	private static void TryAddFace( ImportContext context, ushort faceIndex, ref HashSet<ushort> faceIndices )
	{
		context.Geometry.TryGetFace( faceIndex, out var face );
		faceIndices.Add( faceIndex );
	}
}
