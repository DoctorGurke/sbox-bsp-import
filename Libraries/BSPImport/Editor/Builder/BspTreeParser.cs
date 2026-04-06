using Editor.MovieMaker;
using System.IO.Compression;

namespace BspImport.Builder;

public class BspTreeParser
{
	public class TreeParseResult
	{
		public List<ushort> FaceIndices = new();
	}

	private ImportContext Context { get; set; }
	private List<int>[]? FaceLeavesLookup { get; set; }

	public BspTreeParser( ImportContext context )
	{
		Context = context;
	}

	/// <summary>
	/// Traverse the bsp tree to find the leaf index for a given point in world space.
	/// </summary>
	/// <param name="point"></param>
	/// <returns>-1 if not found, otherwise the leaf index.</returns>
	public int FindLeafIndex( Vector3 point )
	{
		int nodeIndex = 0; // Start at headnode (model 0)  
		while ( nodeIndex >= 0 )
		{
			var node = Context.Nodes![nodeIndex];
			var plane = Context.Planes![node.PlaneIndex];

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
	/// <returns></returns>
	public TreeParseResult GetUniqueWorldspawnFaces()
	{
		var result = new TreeParseResult();

		if ( !Context.HasCompleteGeometry( out var geo ) )
			return result;

		var faces = new HashSet<ushort>();
		ParseNodeFacesRecursively( 0, ref faces );

		result.FaceIndices = faces.ToList();

		return result;
	}

	private void ParseNodeFacesRecursively( int index, ref HashSet<ushort> faceIndices )
	{
		if ( Context.Nodes is null )
			return;

		MapNode node = Context.Nodes[index];

		if ( Context.BuildSettings.CullSkybox && Context.SkyboxAreas.Contains( node.Area ) )
			return;

		// contribute to faces collection
		for ( ushort i = 0; i < node.FaceCount; i++ )
		{
			ushort faceIndex = node.FirstFaceIndex;
			faceIndex += i;

			TryAddFace( faceIndex, ref faceIndices );
		}

		// gather faces from children
		for ( int i = 0; i < 2; i++ )
		{
			var child = node.Children[i];

			// 0 = no child
			if ( child == 0 ) continue;

			// <0 = leaf, not node
			if ( child < 0 )
			{
				AddLeafFaces( -1 - child, ref faceIndices );
				continue;
			}

			// parse child node recursively
			ParseNodeFacesRecursively( child, ref faceIndices );
		}
	}

	private void AddLeafFaces( int index, ref HashSet<ushort> faceIndices )
	{
		if ( Context.Leafs is null )
			return;

		if ( index >= Context.Leafs.Length )
			return;

		var leaf = Context.Leafs[index];

		if ( leaf.WaterDataIndex != -1 )
			return;


		bool isWater = (leaf.Contents & ContentsFlags.Water) == ContentsFlags.Water;
		if ( isWater )
			return;

		//var isWaterLeaf = leaf.WaterDataIndex != -1;
		//var isSkyboxLeaf = (leaf.Flags & 0x01) != 0;

		if ( Context.BuildSettings.CullSkybox && Context.SkyboxAreas.Contains( leaf.Area ) )
			return;

		// contribute to faces collection
		for ( ushort i = 0; i < leaf.FaceCount; i++ )
		{
			ushort leafFaceIndex = leaf.FirstFaceIndex;
			leafFaceIndex += i;

			Context.Geometry.TryGetLeafFaceIndex( leafFaceIndex, out var faceIndex );

			TryAddFace( faceIndex, ref faceIndices );
		}
	}

	private bool TryAddFace( ushort faceIndex, ref HashSet<ushort> faceIndices )
	{
		if ( !Context.Geometry.TryGetFace( faceIndex, out var face ) )
			return false;

		if ( !faceIndices.Add( faceIndex ) )
			return false;

		return true;
	}
}
