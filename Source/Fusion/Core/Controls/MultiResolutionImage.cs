using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.IO;
using System.Linq;

namespace Outracks.Fusion
{
	public class MultiResolutionImage : CachedImageBase
	{
		readonly IImmutableSet<ImageStream> _differentResolutions;

		public MultiResolutionImage(IEnumerable<ImageStream> differentResolutions)
		{
			_differentResolutions = differentResolutions
				.ToImmutableSortedSet(new ImageStreamComparer());
		}

		ImageStream GetImageFromScaleFactor(Ratio<Pixels, Points> scaleFactor)
		{
			foreach (var img in _differentResolutions)
			{
				if (img.ScaleFactor >= scaleFactor)
					return img;
			}

			return _differentResolutions.Last();
		}

		protected override Stream OnCreatePngStream(Ratio<Pixels, Points> scaleFactor, Optional<IColorMap> colorMap, out Ratio<Pixels, Points> imageScaleFactor)
		{
			var imageStream = GetImageFromScaleFactor(scaleFactor);
			imageScaleFactor = imageStream.ScaleFactor;
			return imageStream.Create();
		}

		class ImageStreamComparer : IComparer<ImageStream>
		{
			public int Compare(ImageStream x, ImageStream y)
			{
				return (int)(Math.Round(x.ScaleFactor - y.ScaleFactor, MidpointRounding.AwayFromZero));
			}
		}
	}

	public class ImageStream
	{
		public readonly Ratio<Pixels, Points> ScaleFactor;
		public readonly Func<Stream> Create;

		public ImageStream(Ratio<Pixels, Points> scaleFactor, Func<Stream> create)
		{
			ScaleFactor = scaleFactor;
			Create = create;
		}
	}
}