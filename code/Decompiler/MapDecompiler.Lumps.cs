﻿using BspImport.Decompiler.Lumps;

namespace BspImport.Decompiler;

public partial class MapDecompiler
{
	protected virtual BaseLump? ParseLump( int index, IEnumerable<byte> data, int version )
	{
		switch ( (LumpType)index )
		{
			case LumpType.EntityLump:
				return new EntityLump( Context, data );
			case LumpType.VertexLump:
				return new VertexLump( Context, data );
			case LumpType.FaceLump:
				return new FaceLump( Context, data );
			case LumpType.EdgeLump:
				return new EdgeLump( Context, data );
			case LumpType.SurfaceEdgeLump:
				return new SurfaceEdgeLump( Context, data );
			case LumpType.ModelLump:
				return new ModelLump( Context, data );
			case LumpType.OriginalFaceLump:
				return new OriginalFaceLump( Context, data );
			case LumpType.GameLump:
				return new GameLumpHeader( Context, data );
			default:
				break;
		}
		return null;
	}

	public enum LumpType
	{
		EntityLump = 0,
		VertexLump = 3,
		FaceLump = 7,
		EdgeLump = 12,
		SurfaceEdgeLump = 13,
		ModelLump = 14,
		OriginalFaceLump = 27,
		GameLump = 35,
	}
}

