using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.IO;

namespace Outracks.Fusion
{
	public abstract class CachedImageBase : IImage
	{
		readonly ConcurrentDictionary<CacheKey, object> _cache = new ConcurrentDictionary<CacheKey, object>(1, 1);

		#region Cache key
		struct CacheKey
		{
			readonly Type _platformImageType;
			readonly Optional<IColorMap> _colorMap;
			readonly Ratio<Pixels, Points> _scaleFactor;

			public CacheKey(Type platformImageType, Optional<IColorMap> colorMap, Ratio<Pixels, Points> scaleFactor) : this()
			{
				_platformImageType = platformImageType;
				_colorMap = colorMap;
				_scaleFactor = scaleFactor;
			}

			public override bool Equals(object obj)
			{
				if (!(obj is CacheKey))
				{
					return false;
				}

				var key = (CacheKey)obj;
				return EqualityComparer<Type>.Default.Equals(_platformImageType, key._platformImageType) &&
					   EqualityComparer<Optional<IColorMap>>.Default.Equals(_colorMap, key._colorMap) &&
					   _scaleFactor.Equals(key._scaleFactor);
			}

			public override int GetHashCode()
			{
				var hashCode = 2127219917;
				hashCode = hashCode * -1521134295 + base.GetHashCode();
				hashCode = hashCode * -1521134295 + EqualityComparer<Type>.Default.GetHashCode(_platformImageType);
				hashCode = hashCode * -1521134295 + EqualityComparer<Optional<IColorMap>>.Default.GetHashCode(_colorMap);
				hashCode = hashCode * -1521134295 + EqualityComparer<Ratio<Pixels, Points>>.Default.GetHashCode(_scaleFactor);
				return hashCode;
			}
		}
		#endregion

		public ImageVersion<T> Load<T>(Ratio<Pixels, Points> optimalScaleFactor = default(Ratio<Pixels, Points>), Optional<IColorMap> colorMap = default(Optional<IColorMap>), bool cache = true)
		{
			optimalScaleFactor = optimalScaleFactor.Value == default(double) ? new Ratio<Pixels, Points>(1) : optimalScaleFactor;

			if (!cache || typeof(T) == typeof(Stream))
			{
				Ratio<Pixels, Points> imageScaleRatio;
				var image = OnLoad<T>(optimalScaleFactor, colorMap, out imageScaleRatio);
				return new ImageVersion<T>(imageScaleRatio, image);
			}

			var cacheKey = new CacheKey(typeof(T), colorMap, optimalScaleFactor);
			return (ImageVersion<T>)_cache.GetOrAdd(cacheKey, _ => Load<T>(optimalScaleFactor, colorMap, cache: false));
		}

		T OnLoad<T>(Ratio<Pixels, Points> optimalScaleFactor, Optional<IColorMap> colorMap, out Ratio<Pixels, Points> imageScaleFactor)
		{
			// For testing purposes
			if (typeof(T) == typeof(Stream))
			{
				return (T)(object)OnCreatePngStream(optimalScaleFactor, colorMap, out imageScaleFactor);
			}

			var fromStream = Image.Implementation.Loader<T>.FromStream;

			if (fromStream == null)
				throw new InvalidOperationException("Image loader for " + typeof(T).FullName + " is not registered");

			using (Stream stream = OnCreatePngStream(optimalScaleFactor, colorMap, out imageScaleFactor))
			{
				return fromStream(stream);
			}
		}

		protected abstract Stream OnCreatePngStream(Ratio<Pixels, Points> scaleFactor, Optional<IColorMap> colorMap, out Ratio<Pixels, Points> imageScaleFactor);
	}
}