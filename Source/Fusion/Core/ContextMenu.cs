using System;

namespace Outracks.Fusion
{
	public static class ContextMenu
	{
		
		public static IControl SetContextMenu(this IControl control, Optional<Menu> menu)
		{
			return Implementation.Set(control, menu);
		}


		public static class Implementation
		{
			public static Func<IControl, Optional<Menu>, IControl> Set;
		}
	}
}