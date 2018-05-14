using System;
using System.ComponentModel;
using System.Drawing;
using System.Linq;
using AppKit;
using CoreGraphics;
using Foundation;

namespace Outracks.Fusion.OSX
{
	using IO;

	public static class Marshalling
	{
		public static NSEventModifierMask ToNSEventModifierMask(this ModifierKeys keys)
		{
			var mask = (NSEventModifierMask)0;

			if (keys.HasFlag (ModifierKeys.Alt)) 
				mask |= NSEventModifierMask.AlternateKeyMask;
			if (keys.HasFlag (ModifierKeys.Command))
				mask |= NSEventModifierMask.CommandKeyMask;
			if (keys.HasFlag (ModifierKeys.Control)) 
				mask |= NSEventModifierMask.ControlKeyMask;
			if (keys.HasFlag (ModifierKeys.Shift)) 
				mask |= NSEventModifierMask.ShiftKeyMask;
			if(keys.HasFlag(ModifierKeys.Meta))
				mask |= NSEventModifierMask.CommandKeyMask;
			
			return mask;
		}
		
		public static string ToKeyEquivalent (this Key key)
		{
			switch (key) 
			{
				case Key.None: return "";
				case Key.Space: return " ";
				case Key.D0: return "0";
				case Key.D1: return "1";
				case Key.D2: return "2";
				case Key.D3: return "3";
				case Key.D4: return "4";
				case Key.D5: return "5";
				case Key.D6: return "6";
				case Key.D7: return "7";
				case Key.D8: return "8";
				case Key.D9: return "9";
				case Key.Plus: return "+";
				case Key.Minus: return "-";
				case Key.NumPad0: return "0";
				case Key.NumPad1: return "1";
				case Key.NumPad2: return "2";
				case Key.NumPad3: return "3";
				case Key.NumPad4: return "4";
				case Key.NumPad5: return "5";
				case Key.NumPad6: return "6";
				case Key.NumPad7: return "7";
				case Key.NumPad8: return "8";
				case Key.NumPad9: return "9";
				case Key.Add: return "+";
				case Key.Subtract: return "-";
			}
			return key.ToString ().ToLower ();
		}
		
		public static Color ToColor(this NSColor value)
		{
			if(value == null)
				return new Color(0,0,0,1);
			
			var calibratedColor = value.UsingColorSpace(NSColorSpace.DeviceRGBColorSpace);
			return new Color((float)calibratedColor.RedComponent, (float)calibratedColor.GreenComponent, (float)calibratedColor.BlueComponent, (float)calibratedColor.AlphaComponent);
		}

		public static NSColor ToNSColor(this Color value)
		{
			return NSColor.FromColorSpace(NSColorSpace.SRGBColorSpace, new nfloat[] { value.R, value.G, value.B, value.A });
		}

		public static nfloat[] ToLineDash(this StrokeDashArray value)
		{
			return value.Data.Select(v => (nfloat)v).ToArray();
		}

		public static NSTextAlignment ToNSTextAlignment(this TextAlignment alignment)
		{
			switch (alignment)
			{
				case TextAlignment.Left:
					return NSTextAlignment.Left;
				case TextAlignment.Center:
					return NSTextAlignment.Center;
				case TextAlignment.Right:
					return NSTextAlignment.Right;
				default:
					throw new InvalidEnumArgumentException("alignment", (int)alignment, typeof(TextAlignment));
			}
		}
		public static Point<Points> ToFusion(this CGPoint p)
		{
			return Point.Create<Points>((double)p.X, (double)p.Y);
		}

		public static Size<Points> ToFusion(this CGSize size)
		{
			return Size.Create<Points>((double)size.Width, (double)size.Height);
		}

		public static CGSize ToSize(this Size<Points> size)
		{
			return new CGSize(size.Width, size.Height);
		}

		public static CGPoint ToPoint(this Point<Points> p)
		{
			return new CGPoint(p.X, p.Y);
		}

		public static AbsoluteFilePath ToAbsoluteFilePath(this NSUrl url)
		{
			return AbsoluteFilePath.Parse(url.Path);
		}

		public static AbsoluteDirectoryPath ToAbsoluteDirectoryPath(this NSUrl url)
		{
			return AbsoluteDirectoryPath.Parse(url.Path);
		}

		public static NSUrl ToNSUrl(this AbsoluteFilePath path)
		{
			return new NSUrl(new Uri(path.NativePath).AbsoluteUri);
		}

		public static NSUrl ToNSUrl(this AbsoluteDirectoryPath path)
		{
			return new NSUrl(new Uri(path.NativePath).AbsoluteUri);
		}

		public static NSLineBreakMode ToNSLineBreakMode(this LineBreakMode lineBreakMode)
		{
			switch (lineBreakMode)
			{
				case LineBreakMode.Clip:
					return NSLineBreakMode.Clipping;
				case LineBreakMode.Wrap:
					return NSLineBreakMode.ByWordWrapping;
				case LineBreakMode.TruncateHead:
					return NSLineBreakMode.TruncatingHead;
				case LineBreakMode.TruncateTail:
					return NSLineBreakMode.TruncatingTail;
				default:
					throw new InvalidEnumArgumentException("lineBreakMode", (int)lineBreakMode, typeof(LineBreakMode));
			}
		}
	}
}
