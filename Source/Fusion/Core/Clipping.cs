using System;

namespace Outracks.Fusion
{

	public static class Clipping
	{
		static Func<IControl,bool, IControl> _clip = (c, b) => c;

		public static void Initialize(Func<IControl, bool, IControl> clip)
		{
			_clip = clip;
		}
		
		public static IControl Clip(this IControl control)
		{
			return _clip(control, true);
		}

		public static IControl Overflow(this IControl control)
		{
			return _clip(control, false);
		}

	}
}