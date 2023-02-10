namespace BspImport.Builder;

public static class TreeParse
{
	public static HashSet<int> ParseTreeFaces( ImportContext context )
	{
		var faces = new HashSet<int>();
		ParseNodeFacesRecursively( context, 0, ref faces );
		return faces;
	}

	private static void ParseNodeFacesRecursively( ImportContext context, int index, ref HashSet<int> faceIndices, int depth = 0 )
	{
		if ( context.Nodes is null )
			return;

		var node = context.Nodes[index];

		//var padding = string.Concat( Enumerable.Repeat( "\t", depth ) );
		//Log.Info( $"{padding}@ adding faces for node {index}" );
		//Log.Info( $"{padding}\t* {node.Children[0]}" );
		//Log.Info( $"{padding}\t* {node.Children[1]}" );

		// contribute to faces collection
		for ( int i = 0; i < node.FaceCount; i++ )
		{
			var face = node.FirstFaceIndex + i;
			faceIndices.Add( face );
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
				AddLeafFaces( context, -1 - child, ref faceIndices, depth + 1 );
				continue;
			}

			// parse child node recursively
			ParseNodeFacesRecursively( context, child, ref faceIndices, depth + 1 );
		}
	}

	private static void AddLeafFaces( ImportContext context, int index, ref HashSet<int> faceIndices, int depth )
	{
		//var padding = string.Concat( Enumerable.Repeat( "\t", depth ) );
		if ( context.Leafs is null )
			return;

		if ( index >= context.Leafs.Length )
			return;

		var leaf = context.Leafs[index];
		//Log.Info( $"{padding}- adding faces for leaf {index} {leaf.FirstFaceIndex} {leaf.FaceCount}" );

		// contribute to faces collection
		for ( int i = 0; i < leaf.FaceCount; i++ )
		{
			var faceIndex = leaf.FirstFaceIndex + i;
			var face = context.Geometry.LeafFaceIndices?[faceIndex];

			if ( face is null ) continue;

			//Log.Info( $"{padding} '- {faceIndex} -> {face}" );
			faceIndices.Add( (int)face );
		}
	}
}
