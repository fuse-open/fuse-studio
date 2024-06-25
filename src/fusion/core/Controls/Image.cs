using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Drawing.Imaging;
using System.IO;
using System.Linq;
using System.Net;
using System.Reactive.Linq;
using System.Reflection;
using System.Text.RegularExpressions;

namespace Outracks.Fusion
{
	public static class Image
	{
		static readonly ConcurrentDictionary<string, IImage> _imageCache = new ConcurrentDictionary<string, IImage>();
		static readonly ConcurrentDictionary<Assembly, ImmutableHashSet<string>> _resourceCaches = new ConcurrentDictionary<Assembly, ImmutableHashSet<string>>();

		static Optional<Func<Stream>> GetResource(string resourceName, Assembly assembly)
		{
			var resourceNames = _resourceCaches.GetOrAdd(assembly, a => a.GetManifestResourceNames().ToImmutableHashSet());
			if (!resourceNames.Contains(resourceName))
				return Optional.None();

			return Optional.Some<Func<Stream>>(() => assembly.GetManifestResourceStream(resourceName));
		}

		static Optional<ImageStream> GetImageStreamFromResource(string resourceName, Assembly assembly, int scaleFactor)
		{
			return GetResource(resourceName, assembly).Select(s => new ImageStream(scaleFactor, s));
		}

		public static bool HasResource(string resourceName, Assembly assembly)
		{
			return GetImageStreamFromResource(resourceName, assembly, 1).HasValue;
		}

		public static IControl FromImage(IObservable<IImage> image, IObservable<IColorMap> colorMap = null, IObservable<Ratio<Pixels, Points>> dpiOverride = null)
		{
			return Implementation.Factory(
				image,
				Optional.None(),
				colorMap.ToOptional(),
				dpiOverride.ToOptional());
		}

		public static IImage GetImageFromResource(string resourceName, Assembly assembly)
		{
			return _imageCache.GetOrAdd(
				string.Format("{0}%{1}", assembly.FullName, resourceName),
				_ =>
				{
					if (resourceName.EndsWith(".svg"))
					{
						var resourceStreamFactory = GetResource(resourceName, assembly);
						if (!resourceStreamFactory.HasValue)
							throw new ArgumentException(
								"Resource '" + resourceName + "' not found in assembly " + assembly.GetName().FullName);

						return new SvgImage(resourceStreamFactory.Value);
					}

					var streams = new List<ImageStream>();
					GetImageStreamFromResource(resourceName, assembly, 1).Do(streams.Add);
					var highDpiImageName = Path.GetFileNameWithoutExtension(resourceName) + "@2x.png";
					GetImageStreamFromResource(highDpiImageName, assembly, 2).Do(streams.Add);

					if (streams.IsEmpty())
						throw new ArgumentException(
							"Resource '" + resourceName + "' or with postfix ('@2x') not found in assembly " + assembly.GetName().FullName);
					return new MultiResolutionImage(streams);
				});
		}

		public static IControl FromResource(string resourceName, Assembly assembly, IObservable<Color> overlayColor = null, IObservable<IColorMap> colorMap = null)
		{
			return Implementation.Factory(Observable.Return(GetImageFromResource(resourceName, assembly)), overlayColor.ToOptional(), colorMap.ToOptional(), Optional.None());
		}

		class AnimationImageNameComparer : IComparer<string>
		{
			readonly string filenamePattern = @"^(?<prefix>.*?)(?<number>\d+)(?<suffix>\D*)$";
			public int Compare(string x, string y)
			{
				if (x == y) return 0;
				var xMatch = Regex.Match(x, filenamePattern);
				var yMatch = Regex.Match(y, filenamePattern);
				var prefix = Comparer<string>.Default.Compare(
					xMatch.Groups["prefix"].Value,
					yMatch.Groups["prefix"].Value);
				if (prefix != 0) return prefix;
				var number = Comparer<int>.Default.Compare(
					int.Parse(xMatch.Groups["number"].Value),
					int.Parse(yMatch.Groups["number"].Value));
				if (number != 0) return number;
				return Comparer<string>.Default.Compare(
					xMatch.Groups["suffix"].Value,
					yMatch.Groups["suffix"].Value);
			}
		}

		/// <summary>
		///		Gathers all images under the given resource directory and plays them over the given duration.
		///		Assumes that images are named on the form Foo-1.png, Foo-2.png, ...., Foo-n.png
		///		<todo>Probably need some more error handling. </todo>
		/// </summary>
		/// <param name="resourceDirectoryName">Where to look for images</param>
		/// <param name="assembly">Assembly to find the embeded images resources</param>
		/// <param name="duration">How long one loop lasts</param>
		/// <param name="overlayColor">Tint all images with the provided color.</param>
		/// <returns></returns>
		public static IControl Animation(string resourceDirectoryName, Assembly assembly, TimeSpan duration, IObservable<Color> overlayColor = null)
		{
			var resourceCache = _resourceCaches.GetOrAdd(assembly, a => a.GetManifestResourceNames().ToImmutableHashSet());
			var resources = resourceCache
				.Where(k => k.Contains(resourceDirectoryName))
				.ToImmutableSortedSet(new AnimationImageNameComparer())
				.Select(resource => new ImageStream(scaleFactor: 1, create: () => assembly.GetManifestResourceStream(resource)))
				.ToList();
			return Implementation.Animate(resources, duration, overlayColor);
		}


		public static Optional<IControl> FromUrl(Uri url, IObservable<Color> tint = null)
		{
			try
			{
				byte[] imageData;

				using (var wc = new WebClient())
					imageData = wc.DownloadData(url);
				var imageStream = new ImageStream(scaleFactor: 1, create: () => new MemoryStream(imageData));

				return Optional.Some(Implementation.Factory(
					Observable.Return<IImage>(new MultiResolutionImage(new[] { imageStream })),
					tint.ToOptional(),
					Optional.None(),
					Optional.None()));
			}
			catch (WebException)
			{
				Console.WriteLine("Failed to download " + url);
				return Optional.None();
			}
		}

		public static IControl FromBitmaps(System.Drawing.Image[] bitmaps, IObservable<Color> tint = null)
		{
			var streams = bitmaps
				.Select((bitmap, index) =>
					new ImageStream(scaleFactor: new Ratio<Pixels, Points>(index), create: bitmap.CreatePngStreamFromBitmap));

			return Implementation.Factory(
				Observable.Return<IImage>(new MultiResolutionImage(streams)),
				tint.ToOptional(),
				Optional.None<IObservable<IColorMap>>(),
				Optional.None());
		}

		static Stream CreatePngStreamFromBitmap(this System.Drawing.Image bitmap)
		{
			var memoryStream = new MemoryStream();
			bitmap.Save(memoryStream, ImageFormat.Png);
			memoryStream.Seek(0, SeekOrigin.Begin);
			return memoryStream;
		}

		public static class Implementation
		{
			public static class Loader<TImage>
			{
				public static Func<Stream, TImage> FromStream;
			}

			public static Func<IObservable<IImage>, Optional<IObservable<Color>>, Optional<IObservable<IColorMap>>, Optional<IObservable<Ratio<Pixels, Points>>>, IControl> Factory;

			public static Func<IList<ImageStream>, TimeSpan, IObservable<Color>, IControl> Animate;
		}
	}
}
