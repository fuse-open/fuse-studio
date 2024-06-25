using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Reactive.Linq;
using System.Reactive.Subjects;
using Outracks.Fuse.Auth;
using Outracks.Fusion;
using Outracks.Fusion.Dialogs;
using Outracks.Simulator.Bytecode;

namespace Outracks.Fuse.Stage
{
	class StageController : IStage, IDisposable
	{
		readonly BehaviorSubject<IImmutableList<IViewport>> _viewports = new BehaviorSubject<IImmutableList<IViewport>>(ImmutableList<IViewport>.Empty);
		readonly BehaviorSubject<Optional<IViewport>> _focusedViewport = new BehaviorSubject<Optional<IViewport>>(Optional.None());

		readonly ViewportFactory _viewportFactory;
		readonly PreviewDevices _previewDevices;
		readonly IProperty<VirtualDevice> _latestDevice;

		readonly IProperty<bool> _selectionEnabled;
		readonly ILicense _license;

		public StageController(
			ViewportFactory viewportFactory,
			PreviewDevices previewDevices,
			IProperty<bool> selectionEnabled,
			ILicense license)
		{
			_viewportFactory = viewportFactory;
			_previewDevices = previewDevices;
			_selectionEnabled = selectionEnabled;

			_license = license;
			_license.Updated += LicenseUpdated;

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
			return Menu.Toggle(Texts.SubMenu_Viewport_Selection, _selectionEnabled, HotKey.Create(ModifierKeys.Meta, Key.I))
				+ Menu.Separator
				+ Menu.Item(Texts.SubMenu_Viewport_New, NewViewport, hotkey: HotKey.Create(ModifierKeys.Meta, Key.T))
				+ Menu.Item(Texts.SubMenu_Viewport_Close, CloseFocusedViewport, hotkey: HotKey.Create(ModifierKeys.Meta, Key.W))
				+ Menu.Separator
				+ Menu.Item(Texts.SubMenu_Viewport_Restart, RestartViewport(viewport))
				+ Menu.Separator
				+ DevicesMenu.Create(Strings.SubMenu_Viewport_Device, _latestDevice, _previewDevices)
				+ Menu.Item(Texts.SubMenu_Viewport_FlipAspect,
						// Moved here from DevicesMenu.
						command: Command.Enabled(() =>
							_latestDevice
								.Select(device => device.With(
									orientation: device.Orientation == DeviceOrientation.Landscape
										? DeviceOrientation.Portrait
										: DeviceOrientation.Landscape))
								.Take(1)
								.Subscribe(device => _latestDevice.Write(device))),
						hotkey: HotKey.Create(ModifierKeys.Meta, Key.F))
				+ Menu.Item(Texts.SubMenu_Viewport_Goback, GoBack, hotkey: HotKey.Create(ModifierKeys.Meta, Key.B));
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
			_license.Updated -= LicenseUpdated;

			foreach (var viewport in _viewports.Value)
			{
				viewport.Close();
			}
		}

		void LicenseUpdated(object sender, LicenseStatus status)
		{
			Application.MainThread.InvokeAsync(() => {
				Console.WriteLine("License updated; restarting viewports");

				for (int i = 0; i < _viewports.Value.Count; i++)
				{
					try
					{
						var vp = _viewports.Value[i];
						var newViewport = CreateViewport(vp.VirtualDevice.Value);
						var newViewports = _viewports.Value.RemoveAt(i).Insert(i, newViewport);
						_viewports.OnNext(newViewports);
						if (_focusedViewport.Value.Value == vp)
							_focusedViewport.OnNext(Optional.Some(newViewport));
						vp.Close();
					}
					catch (Exception e)
					{
						Console.Error.WriteLine(e);
					}
				}

				return true;
			});
		}
	}
}
