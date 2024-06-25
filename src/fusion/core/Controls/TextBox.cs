using System;

namespace Outracks.Fusion
{
	[Flags]
	public enum StyleFlags
	{
		None = 0,
		InvalidValue = 1<<0,
	}

	public static class TextBox
	{
		public static IControl Create(
			IProperty<string> text = null,
			bool isMultiline = false,
			bool doWrap = false,
			Optional<Command> onFocused = default(Optional<Command>),
			Brush foregroundColor = default(Brush))
		{
			return Implementation.Factory(
				text ?? Property.Create(""),
				isMultiline,
				doWrap,
				onFocused,
				foregroundColor | Color.White);
		}

		public static class Implementation
		{
			public static Func<IProperty<string>, bool, bool, Optional<Command>, Brush, IControl> Factory;
		}
	}
}