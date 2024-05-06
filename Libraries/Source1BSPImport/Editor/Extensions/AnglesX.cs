namespace BspImport.Extensions;

public static class AnglesX
{
	public static bool AlmostEqual( this Angles angles, Angles other, float delta = 0.0001f )
	{
		if ( Math.Abs( angles.pitch - other.pitch ) > delta )
		{
			return false;
		}

		if ( Math.Abs( angles.yaw - other.yaw ) > delta )
		{
			return false;
		}

		if ( Math.Abs( angles.roll - other.roll ) > delta )
		{
			return false;
		}

		return true;
	}
}
