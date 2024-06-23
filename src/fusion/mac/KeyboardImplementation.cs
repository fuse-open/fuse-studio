using System.Reactive;
using System.Reactive.Concurrency;
using System.Reactive.Linq;
using System.Reactive.Subjects;
using System.Reactive.Threading.Tasks;
using AppKit;

namespace Outracks.Fusion.Mac
{
	static class KeyboardImplementation
	{
		public static void Initialize(IScheduler dispatcher)
		{
			Keyboard.Implementation.GiveFocusTo = (control) => Command.Disabled;

			Keyboard.Implementation.Pressed = (self, modifier, key) =>
				Fusion.Application.MainThread
					.InvokeAsync(() => self.NativeHandle)
					.ToObservable()
					.OfType<IObservableResponder>()
					.Select(view => view.KeyDown).Switch()
					.Where(evt => evt.CharactersIgnoringModifiers == key.ToKeyEquivalent()
						&& evt.ModifierFlags.HasFlag(modifier.ToNSEventModifierMask()))
					.Select(c => Unit.Default);

			Keyboard.Implementation.Released = (self, modifier, key) =>
				Fusion.Application.MainThread
					.InvokeAsync(() => self.NativeHandle)
					.ToObservable()
					.OfType<IObservableResponder>()
					.Select(view => view.KeyUp).Switch()
					.Where(evt => evt.CharactersIgnoringModifiers == key.ToKeyEquivalent()
						&& evt.ModifierFlags.HasFlag(modifier.ToNSEventModifierMask()))
					.Select(c => Unit.Default);

			Keyboard.Implementation.GlobalPressed = (self, modifier, key) =>
			{
				var keyDown = new Subject<NSEvent>();

				NSEvent.AddLocalMonitorForEventsMatchingMask(NSEventMask.KeyDown,
					evt =>
					{
						keyDown.OnNext(evt);
						return evt;
					});

				return keyDown
					.Where(evt => evt.CharactersIgnoringModifiers == key.ToKeyEquivalent()
						&& evt.ModifierFlags.HasFlag(modifier.ToNSEventModifierMask()))
					.Select(c => Unit.Default);
			};

			Keyboard.Implementation.GlobalReleased = (self, modifier, key) =>
			{
				var keyUp = new Subject<NSEvent>();

				NSEvent.AddLocalMonitorForEventsMatchingMask(NSEventMask.KeyUp,
					evt =>
					{
						keyUp.OnNext(evt);
						return evt;
					});

				return keyUp
					.Where(evt => evt.CharactersIgnoringModifiers == key.ToKeyEquivalent()
						&& evt.ModifierFlags.HasFlag(modifier.ToNSEventModifierMask()))
					.Select(c => Unit.Default);
			};
		}
	}
}