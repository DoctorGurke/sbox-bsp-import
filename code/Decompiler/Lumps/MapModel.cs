using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BspImport.Decompiler.Lumps;

public class MapModel
{
	public Vector3 Mins;
	public Vector3 Maxs;
	public Vector3 Origin;
	public int HeadNode;
	public int FirstFace;
	public int NumFaces;

	public MapModel( Vector3 mins, Vector3 maxs, Vector3 origin, int headnode, int firstface, int numfaces )
	{
		Mins = mins;
		Maxs = maxs;
		Origin = origin;
		HeadNode = headnode;
		FirstFace = firstface;
		NumFaces = numfaces;
	}
}
