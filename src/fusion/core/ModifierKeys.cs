using System;

namespace Outracks.Fusion
{
	[Flags]
	public enum ModifierKeys
	{
		None = 0,
		Alt = 1 << 0,
		Control = 1 << 1,
		Shift = 1 << 2,
		Windows = 1 << 3,
		Command = 1 << 3,
		Meta = 1 << 4,
	}
}
