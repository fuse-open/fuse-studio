using System;
using System.Reactive;
using System.Reactive.Concurrency;
using System.Reactive.Linq;
using System.Reactive.Subjects;
using System.Reactive.Threading.Tasks;
using System.Windows;
using System.Windows.Forms.Integration;
using System.Windows.Input;

namespace Outracks.Fusion.Windows
{
	static class KeyboardImplementation
	{
		public static void Initialize(Dispatcher dispatcher)
		{
			Keyboard.Implementation.GiveFocusTo = control =>
				Fusion.Application.MainThread
					.InvokeAsync(() => control.NativeHandle)
					.ToObservable()
					.OfType<FrameworkElement>()
					.Switch(element =>
						Command.Enabled(() => 
							dispatcher.Schedule(() =>
							{
								element.Focus();
								System.Windows.Input.Keyboard.Focus(element);
							})));

			Keyboard.Implementation.Pressed = CreateHandler((e, h) => e.KeyDown += h, (e, h) => e.KeyDown -= h);
			Keyboard.Implementation.Released = CreateHandler((e, h) => e.KeyUp += h, (e, h) => e.KeyUp -= h);

			Keyboard.Implementation.GlobalPressed = CreateHandlerGlobal(dispatcher, (e, h) => e.PreviewKeyDown += h, (e, h) => e.PreviewKeyDown -= h);
			Keyboard.Implementation.GlobalReleased = CreateHandlerGlobal(dispatcher, (e, h) => e.PreviewKeyUp += h, (e, h) => e.PreviewKeyUp -= h);
		}

		static Func<IControl, ModifierKeys, Key, IObservable<Unit>> CreateHandler(
			Action<FrameworkElement, KeyEventHandler> addHandler,
			Action<FrameworkElement, KeyEventHandler> removeHandler)
		{
			return (self, modifier, key) => 
				self.GetNativeControlWhileMounted() // The observable sequence is scheduled on the main-thread so it's safe
					.Switch(me => me.MatchWith(
						none: Observable.Never<Unit>,
						some: e => Observable
							.FromEventPattern<KeyEventHandler, KeyEventArgs>(h => addHandler(e, h), h => removeHandler(e, h))
							.Where(a => a.EventArgs.Key == key.ToWpfKey() && System.Windows.Input.Keyboard.Modifiers == modifier.ToWpfModifierKeys())
							.Select(_ => Unit.Default)));
		}

		static Func<IControl, ModifierKeys, Key, IObservable<Unit>> CreateHandlerGlobal(
			Dispatcher dispatcher,
			Action<System.Windows.Window, KeyEventHandler> addHandler,
			Action<System.Windows.Window, KeyEventHandler> removeHandler)
		{
			return (self, modifier, key) =>
				Fusion.Application.MainThread
					.InvokeAsync(() => self.NativeHandle)
					.ToObservable()
					.OfType<FrameworkElement>()
					.Select(element => 
						element.GetWindow<System.Windows.Window>()
							.NotNone()
							.Switch(w => Observable
								.FromEventPattern<KeyEventHandler, KeyEventArgs>(h => addHandler(w, h), h => removeHandler(w, h))
								.Where(a => a.EventArgs.Key == key.ToWpfKey() && System.Windows.Input.Keyboard.Modifiers == modifier.ToWpfModifierKeys())
								.Select(_ => Unit.Default)))
					.Switch();
		}
	}
}