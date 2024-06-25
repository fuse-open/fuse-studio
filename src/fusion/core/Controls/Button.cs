using System;
using System.Reactive.Linq;

namespace Outracks.Fusion
{
	public class ButtonStates
	{
		public static ButtonStates Unrooted = new ButtonStates(Observable.Return(false), Observable.Return(false), Observable.Return(false));

		public readonly IObservable<bool> IsPressed;
		public readonly IObservable<bool> IsHovered;
		public readonly IObservable<bool> IsEnabled;

		public ButtonStates(IObservable<bool> isPressed, IObservable<bool> isHovered, IObservable<bool> isEnabled)
		{
			IsPressed = isPressed;
			IsHovered = isHovered;
			IsEnabled = isEnabled;
		}
	}

	public static class Button
	{
		public static ButtonStates Switch(this IObservable<ButtonStates> states)
		{
			return new ButtonStates(
				states.Select(s => s.IsPressed).Switch(),
				states.Select(s => s.IsHovered).Switch(),
				states.Select(s => s.IsEnabled).Switch());
		}

		public static IControl Create(
			Command clicked = default(Command),
			Func<ButtonStates, IControl> content = null,
			Text text = default(Text),
			Text toolTip = default(Text),
			bool isDefault = false)
		{
			return Implementation.Factory(clicked, content, text, isDefault).SetToolTip(toolTip);
		}

		public static class Implementation
		{
			public static Func<Command, Func<ButtonStates, IControl>, Text, bool, IControl> Factory;
		}
	}
}