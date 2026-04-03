using Editor.MovieMaker;
using System.IO.Compression;

namespace BspImport.Builder;

public static class TreeParse
{
	public class TreeParseResult
	{
		public List<ushort> FaceIndices = new();
	}

	public static int FindLeafIndex( ImportContext context, Vector3 point )
	{
		int nodeIndex = 0; // Start at headnode (model 0)  
		while ( nodeIndex >= 0 )
		{
			var node = context.Nodes![nodeIndex];
			var plane = context.Planes![node.PlaneIndex];

			float distance = plane.Normal.x * point.x +
							plane.Normal.y * point.y +
							plane.Normal.z * point.z - plane.Distance;

			nodeIndex = distance >= 0 ? node.Children[0] : node.Children[1];
		}

		return -1 - nodeIndex; // Convert negative leaf index to positive  
	}

	/// <summary>
	/// Get all unique Face indices from the BSP tree. Results represent render meshes, not brushes. Never brushes.
	/// </summary>
	/// <param name="context"></param>
	/// <returns></returns>
	public static TreeParseResult ParseTreeFaces( ImportContext context )
	{
		var result = new TreeParseResult();

		if ( !context.HasCompleteGeometry( out var geo ) )
			return result;

		var faces = new HashSet<ushort>();
		ParseNodeFacesRecursively( context, 0, ref faces );

		result.FaceIndices = faces.ToList();

		return result;
	}

	private static void ParseNodeFacesRecursively( ImportContext context, int index, ref HashSet<ushort> faceIndices )
	{
		if ( context.Nodes is null )
			return;

		MapNode node = context.Nodes[index];

		if ( context.Settings.CullSkybox && context.SkyboxAreas.Contains( node.Area ) )
			return;

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

		if ( leaf.WaterDataIndex != -1 )
			return;


		bool isWater = (leaf.Contents & ContentsFlags.Water) == ContentsFlags.Water;
		if ( isWater )
			return;

		//var isWaterLeaf = leaf.WaterDataIndex != -1;
		//var isSkyboxLeaf = (leaf.Flags & 0x01) != 0;

		if ( context.Settings.CullSkybox && context.SkyboxAreas.Contains( leaf.Area ) )
			return;

		// contribute to faces collection
		for ( ushort i = 0; i < leaf.FaceCount; i++ )
		{
			ushort leafFaceIndex = leaf.FirstFaceIndex;
			leafFaceIndex += i;

			context.Geometry.TryGetLeafFaceIndex( leafFaceIndex, out var faceIndex );

			TryAddFace( context, faceIndex, ref faceIndices );
		}
	}

	private static bool TryAddFace( ImportContext context, ushort faceIndex, ref HashSet<ushort> faceIndices )
	{
		if ( !context.Geometry.TryGetFace( faceIndex, out var face ) )
			return false;

		if ( !faceIndices.Add( faceIndex ) )
			return false;

		return true;
	}
}
