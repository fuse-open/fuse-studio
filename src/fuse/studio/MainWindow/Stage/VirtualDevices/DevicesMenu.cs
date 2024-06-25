using System;
using System.Linq;
using System.Reactive.Linq;
using Outracks.Fusion;

namespace Outracks.Fuse.Stage
{
	public static class DevicesMenu
	{
		public static Menu Create(
			string localizedName,
			IProperty<VirtualDevice> currentDevice,
			PreviewDevices previewDevices)
		{
			return Menu.Submenu(localizedName,
				previewDevices.Devicess.Select(devices => devices.Select((dev, j) =>
						Menu.Option(
							name: dev.Name,
							value: dev,
							hotkey: GetHotKey(j),
							property: currentDevice.Convert(
								convert: info => info.Screen,
								convertBack: screen => new VirtualDevice(screen, screen.DefaultOrientation)))))
					.Concat()
				+ Menu.Separator
				+ Menu.Item("Customize...", previewDevices.CustomizeDevices()));
		}

		static HotKey GetHotKey(int index)
		{
			if (index > 9)
				return HotKey.None;

			return HotKey.Create(ModifierKeys.Meta, GetNumberKeyFromInt(index));
		}

		static Key GetNumberKeyFromInt(int idx)
		{
			switch (idx)
			{
				default: return Key.D0;
				case 1: return Key.D1;
				case 2: return Key.D2;
				case 3: return Key.D3;
				case 4: return Key.D4;
				case 5: return Key.D5;
				case 6: return Key.D6;
				case 7: return Key.D7;
				case 8: return Key.D8;
				case 9: return Key.D9;
			}
		}
	}
}
