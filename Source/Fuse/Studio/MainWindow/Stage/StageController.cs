using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Reactive.Linq;
using System.Reactive.Subjects;
using Outracks.Fuse.Designer;

namespace Outracks.Fuse.Stage
{
	using Fusion;
	using Simulator.Bytecode;

	class StageController : IStage, IDisposable
	{
		readonly BehaviorSubject<IImmutableList<IViewport>> _viewports = new BehaviorSubject<IImmutableList<IViewport>>(ImmutableList<IViewport>.Empty);
		readonly BehaviorSubject<Optional<IViewport>> _focusedViewport = new BehaviorSubject<Optional<IViewport>>(Optional.None());
		
		readonly ViewportFactory _viewportFactory;
		readonly PreviewDevices _previewDevices;
		readonly IProperty<VirtualDevice> _latestDevice;

		readonly IProperty<bool> _selectionEnabled;
		
		public StageController(
			ViewportFactory viewportFactory,
			PreviewDevices previewDevices,
			IProperty<bool> selectionEnabled)
		{
			_viewportFactory = viewportFactory;
			_previewDevices = previewDevices;
			_selectionEnabled = selectionEnabled;

			var fallbackDevice = previewDevices.DefaultDevice
				.Select(dev => new VirtualDevice(dev, dev.DefaultOrientation))
				.AsProperty();

			var device = _focusedViewport
				.Select(mv => mv.Select(v => v.VirtualDevice.AsProperty()).Or(fallbackDevice))
				.Switch();

			_latestDevice = device;
		}

		public IObservable<IEnumerable<IViewport>> Viewports
		{
			get { return _viewports; }
		}

		public IObservable<Optional<IViewport>> FocusedViewport
		{
			get { return _focusedViewport; }
		}

		public IObservable<Size<Points>> DefaultViewportSize
		{
			get
			{
				return _latestDevice.Select(x => x.GetOrientedSizeInPoints());
			}
		}

		void OpenViewport(VirtualDevice virtualDevice)
		{
			var viewport = CreateViewport(virtualDevice);

			_viewports.OnNext(_viewports.Value.Add(viewport));
			_focusedViewport.OnNext(Optional.Some(viewport));
		}

		void ViewportFocused(IViewport viewport)
		{
			_focusedViewport.OnNext(Optional.Some(viewport));
		}

		void ViewportClosed(IViewport viewport)
		{
			var newViewports = _viewports.Value.Remove(viewport);
			_focusedViewport.OnNext(newViewports.LastOrNone());
			_viewports.OnNext(newViewports);
		}

		public Menu Menu
		{
			get { return CreateMenu(_focusedViewport); }
		}

		Menu CreateMenu(IObservable<Optional<IViewport>> viewport)
		{
			return Menu.Toggle("Selection", _selectionEnabled, HotKey.Create(ModifierKeys.Meta, Key.I))
				+ Menu.Separator
				+ Menu.Item("New viewport", NewViewport, hotkey: HotKey.Create(ModifierKeys.Meta, Key.T))
				+ Menu.Item("Close viewport", CloseFocusedViewport, hotkey: HotKey.Create(ModifierKeys.Meta, Key.W))
				+ Menu.Separator
				+ Menu.Item("Restart", RestartViewport(viewport))
				+ Menu.Separator
				+ DevicesMenu.Create(_latestDevice, _previewDevices)
				+ Menu.Item("Go back", GoBack, hotkey: HotKey.Create(ModifierKeys.Meta, Key.B));
		}

		Command RestartViewport(IObservable<Optional<IViewport>> viewport)
		{
			return viewport.Switch(vp => 
				Command.Create(
					isEnabled: vp.HasValue, 
					action: () =>
					{
						var index = _viewports.Value.IndexOf(vp.Value);
						if (index == -1) 
							return;

						var newViewport = CreateViewport(vp.Value.VirtualDevice.Value);
						var newViewports = _viewports.Value.RemoveAt(index).Insert(index, newViewport);

						_viewports.OnNext(newViewports);

						if (_focusedViewport.Value == vp)
							_focusedViewport.OnNext(Optional.Some(newViewport));

						vp.Value.Close();
					}));
		}

		IViewport CreateViewport(VirtualDevice virtualDevice)
		{
			return _viewportFactory.Create(
				initialDevice: virtualDevice,
				onFocus: ViewportFocused,
				onClose: ViewportClosed,
				menu: self => CreateMenu(Observable.Return(Optional.Some(self))));
		}

		public Command NewViewport
		{
			get
			{
				return _latestDevice.Switch(latestDevice =>
					Command.Enabled(() => OpenViewport(latestDevice)));
			}
		}

		Command CloseFocusedViewport
		{
			get
			{
				return _focusedViewport.Switch(vp =>
					Command.Create(
						isEnabled: vp.HasValue,
						action: () => vp.Value.Close()));
			}
		}

		Command GoBack
		{
			get
			{
				return _focusedViewport.Switch(viewport =>
					Command.Create(
						isEnabled: viewport.HasValue,
						action: () => viewport.Value.Execute(
							new CallStaticMethod(StaticMemberName.Parse("Fuse.Input.Keyboard.EmulateBackButtonTap")))));
			}
		}

		public void Dispose()
		{
			foreach (var viewport in _viewports.Value)
			{
				viewport.Close();
			}
		}
	}
}
