using System;
using System.Reactive;

namespace Outracks.Fusion
{
	public static class Keyboard
	{
		public static T OnKeyPressed<T>(this T control, ModifierKeys modifier, Key key, Optional<Command> handler = default(Optional<Command>), bool isGlobal = false)
			where T : IControl
		{
			if (handler.HasValue)
				Pressed(control, modifier, key, isGlobal)
					.WithLatestFromBuffered(handler.Value.Execute.ConnectWhile(control.IsRooted), (_, c) => c)
					.ConnectWhile(control.IsRooted)
					.Subscribe(c => c());
			return control;
		}

		public static T OnKeyReleased<T>(this T control, ModifierKeys modifier, Key key, Optional<Command> handler = default(Optional<Command>), bool isGlobal = false)
			where T : IControl
		{
			if (handler.HasValue)
				Released(control, modifier, key, isGlobal)
					.WithLatestFromBuffered(handler.Value.Execute.ConnectWhile(control.IsRooted), (_, c) => c)
					.ConnectWhile(control.IsRooted)
					.Subscribe(c => c());
			return control;
		}

		public static IObservable<Unit> Pressed(IControl control, ModifierKeys modifier, Key key, bool isGlobal)
		{
			return isGlobal ? Implementation.GlobalPressed(control, modifier, key) : Implementation.Pressed(control, modifier, key);
		}

		public static IObservable<Unit> Released(IControl control, ModifierKeys modifier, Key key, bool isGlobal)
		{
			return isGlobal ? Implementation.GlobalReleased(control, modifier, key) : Implementation.Released(control, modifier, key);
		}

		public static Command GiveFocusTo(IControl control)
		{
			return Implementation.GiveFocusTo(control);
		}

		public static class Implementation
		{
			public static Func<IControl, ModifierKeys, Key, IObservable<Unit>> Pressed;
			public static Func<IControl, ModifierKeys, Key, IObservable<Unit>> Released;

			public static Func<IControl, ModifierKeys, Key, IObservable<Unit>> GlobalPressed;
			public static Func<IControl, ModifierKeys, Key, IObservable<Unit>> GlobalReleased;
			public static Func<IControl, Command> GiveFocusTo;
		}
	}
}