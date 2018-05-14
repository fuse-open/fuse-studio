using System;

namespace Outracks.UnoHost
{
	[Flags]
	public enum MouseButtons
	{
		None = 0,
		Left = 1 << 0,
		Middle = 1 << 1,
		Right = 1 << 2,
		X1 = 1 << 3,
		X2 = 1 << 4,
	}
}