using System;

namespace Outracks.Fusion
{
	public static class ColorPicker
	{
		public static Command Open(IProperty<Color> value)
		{
			return Implementation.OpenCommand(value);
		}

		public static class Implementation
		{
			public static Func<IProperty<Color>, Command> OpenCommand;
		}
	}
}