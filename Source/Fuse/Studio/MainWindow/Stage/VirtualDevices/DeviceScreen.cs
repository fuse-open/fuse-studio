using System;
using System.IO;

namespace Outracks.Fuse.Stage
{
	public enum DeviceOrientation
	{
		Portrait, Landscape
	}

	public sealed class DeviceScreen : IEquatable<DeviceScreen>
	{
		public readonly bool IsDefault;

		public readonly string Name;

		readonly Size<Points> _size;
		public Size<Points> SizeInPoints
		{
			get { return _size; }
		}
		
		public Size<Pixels> SizeInPixels
		{
			get { return _size.Mul(PixelsPerPoint); }
		}

		public Size<Inches> SizeInInches
		{
			get { return SizeInPixels.Mul(PhysicalPixelsPerPixel).Div(PhysicalPixelsPerInch); }
		}

		public readonly DeviceOrientation DefaultOrientation;

		public readonly Ratio<Pixels,Points> PixelsPerPoint;
		public readonly Ratio<Pixels,Pixels> PhysicalPixelsPerPixel;
		public readonly Ratio<Pixels,Inches> PhysicalPixelsPerInch;

		public DeviceScreen(
			string name,
			Size<Pixels> size,
			Ratio<Pixels,Points> pixelsPerPoint,
			Ratio<Pixels,Inches> physicalPixelsPerInch,
			double physicalPixelsPerPixel = 1.0, 
			DeviceOrientation defaultOrientation = DeviceOrientation.Portrait, 
			bool isDefault = false)
			: this(
				name,
				size.Div(pixelsPerPoint),
				pixelsPerPoint,
				physicalPixelsPerInch,
				physicalPixelsPerPixel, 
				defaultOrientation, 
				isDefault)
		{ }

		public DeviceScreen(
			string name,
			Size<Points> size,
			Ratio<Pixels,Points> pixelsPerPoint,
			Ratio<Pixels,Inches> physicalPixelsPerInch,
			Ratio<Pixels,Pixels>? physicalPixelsPerPixel = null, 
			DeviceOrientation defaultOrientation = DeviceOrientation.Portrait, 
			bool isDefault = false)
		{
			_size = size;
			PixelsPerPoint = pixelsPerPoint;
			PhysicalPixelsPerPixel = physicalPixelsPerPixel ?? new Ratio<Pixels, Pixels>(1.0);
			DefaultOrientation = defaultOrientation;
			IsDefault = isDefault;
			PhysicalPixelsPerInch = physicalPixelsPerInch;
			Name = name;
		}

		public DeviceScreen With(
			string name = null,
			Size<Points>? size = null,
			Ratio<Pixels, Points>? pixelsPerPoint = null,
			Ratio<Pixels, Inches>? physicalPixelsPerInch = null,
			double? physicalPixelsPerPixel = null,
			DeviceOrientation? defaultOrientation = null)
		{
			return new DeviceScreen(
				name ?? Name,
				size ?? SizeInPoints,
				pixelsPerPoint ?? PixelsPerPoint,
				physicalPixelsPerInch ?? PhysicalPixelsPerInch,
				physicalPixelsPerPixel ?? PhysicalPixelsPerPixel,
				defaultOrientation ?? DefaultOrientation);
		}

		public bool Equals(DeviceScreen other)
		{
			if (ReferenceEquals(null, other)) return false;
			if (ReferenceEquals(this, other)) return true;
			return string.Equals(Name, other.Name) 
				&& _size.Equals(other._size) 
				&& DefaultOrientation == other.DefaultOrientation 
				&& PixelsPerPoint.Equals(other.PixelsPerPoint) 
				&& PhysicalPixelsPerPixel.Equals(other.PhysicalPixelsPerPixel) 
				&& PhysicalPixelsPerInch.Equals(other.PhysicalPixelsPerInch);
		}

		public override bool Equals(object obj)
		{
			if (ReferenceEquals(null, obj)) return false;
			if (ReferenceEquals(this, obj)) return true;
			return obj is DeviceScreen && Equals((DeviceScreen)obj);
		}

		public override int GetHashCode()
		{
			unchecked
			{
				var hashCode = Name.GetHashCode();
				hashCode = (hashCode * 397) ^ _size.GetHashCode();
				hashCode = (hashCode * 397) ^ (int)DefaultOrientation;
				hashCode = (hashCode * 397) ^ PixelsPerPoint.GetHashCode();
				hashCode = (hashCode * 397) ^ PhysicalPixelsPerPixel.GetHashCode();
				hashCode = (hashCode * 397) ^ PhysicalPixelsPerInch.GetHashCode();
				return hashCode;
			}
		}

		public static bool operator ==(DeviceScreen left, DeviceScreen right)
		{
			return Equals(left, right);
		}

		public static bool operator !=(DeviceScreen left, DeviceScreen right)
		{
			return !Equals(left, right);
		}

		public static void Write(BinaryWriter writer, DeviceScreen device)
		{
			writer.Write(device.Name);
			Size.Write(writer, device.SizeInPixels);
			Ratio.Write(writer, device.PixelsPerPoint);
			Ratio.Write(writer, device.PhysicalPixelsPerInch);
			writer.Write(device.PhysicalPixelsPerPixel);
		}

		public static DeviceScreen Read(BinaryReader reader)
		{
			return new DeviceScreen(
				reader.ReadString(),
				Size.Read<Pixels>(reader),
				Ratio.Read<Pixels, Points>(reader),
				Ratio.Read<Pixels, Inches>(reader),
				reader.ReadDouble());
		}
	}
}
