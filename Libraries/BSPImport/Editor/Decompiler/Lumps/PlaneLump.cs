using System;
using System.Collections.Generic;
using System.Text;

namespace BspImport.Decompiler.Lumps;

public class PlaneLump : BaseLump
{
	public PlaneLump( ImportContext context, byte[] data, int version = 0 ) : base( context, data, version )
	{
	}

	protected override void Parse( BinaryReader reader )
	{
		var planeCount = reader.GetLength() / 20;
		var planes = new Plane[planeCount];

		for ( int i = 0; i < planeCount; i++ )
		{
			Vector3 normal = reader.ReadVector3();
			float dist = reader.ReadSingle();
			reader.Skip<int>();

			planes[i] = new Plane( normal, dist );
		}

		Context.Planes = planes;
	}
}
