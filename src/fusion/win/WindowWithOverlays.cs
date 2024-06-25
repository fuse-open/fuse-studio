using System;
using System.Collections.Immutable;
using System.Linq;
using System.Reactive.Linq;
using System.Reactive.Subjects;
using System.Windows.Input;

namespace Outracks.Fusion.Windows
{
	public class WindowWithOverlays : DpiAwareWindow
	{
		readonly BehaviorSubject<IImmutableList<Tuple<HotKey, System.Windows.Controls.MenuItem, ICommand>>> _bindings = new BehaviorSubject<IImmutableList<Tuple<HotKey, System.Windows.Controls.MenuItem, ICommand>>>(ImmutableList<Tuple<HotKey, System.Windows.Controls.MenuItem, ICommand>>.Empty);

		public WindowWithOverlays()
		{
			var hostWindow = this;
			hostWindow._bindings.ObserveOn(Fusion.Application.MainThread).SubscribeUsing(binding => hostWindow.Bind(binding));
		}

		public IDisposable AddOverlay(string name, WindowWithOverlays overlayWindow = null, int zIndex = 0)
		{
			var hostWindow = this;
			return Disposable.Combine(
				overlayWindow == null
					? Disposable.Empty
					: hostWindow._bindings.ObserveOn(Fusion.Application.MainThread).SubscribeUsing(overlayWindow.Bind),
				overlayWindow == null
					? Disposable.Empty
					: overlayWindow._bindings.ObserveOn(Fusion.Application.MainThread).SubscribeUsing(hostWindow.Bind));
		}

		public IDisposable AddInputBinding(
			HotKey hotkey,
			System.Windows.Controls.MenuItem item,
			ICommand command)
		{
			var binding = Tuple.Create(hotkey, item, command);
			_bindings.OnNext(_bindings.Value.Add(binding));
			return Disposable.Create(() => _bindings.OnNext(_bindings.Value.Remove(binding)));
		}

		IDisposable Bind(IImmutableList<Tuple<HotKey, System.Windows.Controls.MenuItem, ICommand>> bindings)
		{
			return Disposable.Combine(
				bindings.Select(
					args =>
					{
						var hotkey = args.Item1;
						var item = args.Item2;
						var command = args.Item3;

						var binding = new InputBinding(command, hotkey.ToWpfGesture());
						item.InputGestureText = ((KeyGesture)binding.Gesture).DisplayString;

						InputBindings.Add(binding);
						return Disposable.Create(() =>
						{
							InputBindings.Remove(binding);
						});
					}));
		}
	}
}