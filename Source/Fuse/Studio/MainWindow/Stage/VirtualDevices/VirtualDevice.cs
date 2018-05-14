using System.IO;

namespace Outracks.Fuse.Stage
{
	public class VirtualDevice
	{
		public readonly DeviceScreen Screen;
		public readonly DeviceOrientation Orientation;

		public VirtualDevice(DeviceScreen screen, DeviceOrientation orientation)
		{
			Screen = screen;
			Orientation = orientation;
		}

		public VirtualDevice With(
			Optional<DeviceScreen> screen = default(Optional<DeviceScreen>),
			Optional<DeviceOrientation> orientation = default (Optional<DeviceOrientation>))
		{
			return new VirtualDevice(
				screen.HasValue ? screen.Value : Screen,
				orientation.HasValue ? orientation.Value : Orientation);
		}

		public static void Write(BinaryWriter writer, VirtualDevice virtualDevice)
		{
			DeviceScreen.Write(writer, virtualDevice.Screen);
			writer.Write((int)virtualDevice.Orientation);
		}

		public static VirtualDevice Read(BinaryReader reader)
		{
			return new VirtualDevice(
				DeviceScreen.Read(reader),
				(DeviceOrientation)reader.ReadInt32());
		}
	}
	
	public static class DeviceExtensions
	{

		public static VirtualDevice Resize(this VirtualDevice virtualDevice, Size<Points> newSize)
		{
			if (newSize == virtualDevice.GetOrientedSizeInPoints())
				return virtualDevice;

			return new VirtualDevice(
				virtualDevice.Screen.With(
					name: newSize.Round().ToString(),
					size: newSize,
					defaultOrientation: virtualDevice.Orientation),
				virtualDevice.Orientation);
		}

		public static Size<Pixels> GetOrientedSizeInPixels(this VirtualDevice virtualDevice)
		{
			return virtualDevice.Orientation != virtualDevice.Screen.DefaultOrientation
				? virtualDevice.Screen.SizeInPixels.SwapWidthAndHeight()
				: virtualDevice.Screen.SizeInPixels;
		}
		public static Size<Points> GetOrientedSizeInPoints(this VirtualDevice virtualDevice)
		{
			return virtualDevice.Orientation != virtualDevice.Screen.DefaultOrientation
				? virtualDevice.Screen.SizeInPoints.SwapWidthAndHeight()
				: virtualDevice.Screen.SizeInPoints;
		}
	}
}