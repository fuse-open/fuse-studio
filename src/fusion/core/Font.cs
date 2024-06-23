using System;
using System.Reactive.Linq;

namespace Outracks.Fusion
{
	public static class ToFontExtension
	{
		public static Font Switch(this IObservable<Font> font)
		{
			// TODO: This won't be correct when introducing other fonts than just the system default like we have now,
			//  but should be correct for the current implementation.
			return Font.SystemDefault(font.Select(f => f.Size).Switch().Replay(1).RefCount(), font.Select(f => f.Bold).Switch().Replay(1).RefCount());
		}
	}

	public sealed class Font
	{
		public static Font SystemDefault(double size, bool bold = false)
		{
			return new Font
			{
				Size = Observable.Return(size),
				Bold = Observable.Return(bold),
			};
		}

		public static Font SystemDefault(IObservable<double> size, IObservable<bool> bold = null)
		{
			return new Font
			{
				Size = size,
				Bold = bold ?? Observable.Return(false),
			};
		}

		public static Font Default
		{
			get
			{
				return new Font
				{
					Size = Observable.Return(13.0),
					Bold = Observable.Return(false),
				};
			}
		}

		public IObservable<bool> Bold { get; private set; }

		public IObservable<double> Size { get; private set; }
	}

	public enum TextAlignment
	{
		Left,
		Right,
		Center
	}
}