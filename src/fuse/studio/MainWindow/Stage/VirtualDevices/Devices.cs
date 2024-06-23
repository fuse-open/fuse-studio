using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.IO;
using System.Linq;
using Newtonsoft.Json;
using Outracks.IO;

namespace Outracks.Fuse.Stage
{
	public sealed class Devices
	{
		public static DeviceScreen Default
		{
			get { return LoadDefaultDevices()[0]; }
		}

		public static void SaveDefaultDevices(Stream dst)
		{
			using (var src = GetDefaultDevicesStream())
			{
				src.CopyTo(dst);
			}
		}

		public static ImmutableList<DeviceScreen> LoadDefaultDevices()
		{
			using (var stream = GetDefaultDevicesStream())
			{
				return LoadDevicesFrom(stream);
			}
		}

		static Stream GetDefaultDevicesStream()
		{
			var self = typeof (Devices);
			var name = "Outracks.Fuse.MainWindow.Stage.VirtualDevices.devices.json";
			var stream = self.Assembly.GetManifestResourceStream(name);
			if (stream == null)
				throw new Exception("Could not find embedded resource '" + name + "' in assembly " + self.Assembly.FullName);

			return stream;
		}

		/// <exception cref="MalformedDeviceInfo"></exception>
		public static ImmutableList<DeviceScreen> LoadDevicesFrom(Stream stream)
		{
			try
			{
				return DeviceJson
					.LoadFrom(stream)
					.Select(ToDeviceInfo)
					.ToImmutableList();
			}
			catch (Exception e)
			{
				throw new MalformedDeviceInfo(e);
			}
		}

		static DeviceScreen ToDeviceInfo(DeviceJson json)
		{
			return new DeviceScreen(
				json.Name,
				new Size<Pixels>(json.Width, json.Height),
				json.PixelsPerPoint,
				json.PhysicalPixelsPerInch,
				json.PhysicalPixelsPerPixel,
				json.DefaultOrientation.ToUpper() == "LANDSCAPE"
					? DeviceOrientation.Landscape
					: DeviceOrientation.Portrait,
				json.IsDefault);
		}
	}

	// ReSharper disable once ClassNeverInstantiated.Local
	class DeviceJson
	{
		public static IEnumerable<DeviceJson> LoadFrom(Stream stream)
		{
			return JsonConvert.DeserializeObject<DeviceJson[]>(stream.ReadToEnd());
		}

		public string Name = "Device";
		public string DefaultOrientation = "Portrait";
		public int Width = 100;
		public int Height = 100;
		public float PixelsPerPoint = 1f;
		public float PhysicalPixelsPerPixel = 1f;
		public float PhysicalPixelsPerInch = 1f; //Nonsense default value really. Not sure what a good one is.
		public bool IsDefault = false;
	}

}

