using System;

namespace Outracks.Fusion
{
	public static class ToolTip
	{
		public static IControl SetToolTip(this IControl control, Text toolTip)
		{
			if (toolTip.IsDefault)
				return control;

			return Implementation.Set(control, toolTip);
		}

		public static class Implementation
		{
			public static Func<IControl, Text, IControl> Set;
		}
	}
}
