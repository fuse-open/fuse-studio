using System;
using System.Collections.Concurrent;
using System.Reactive;
using System.Reactive.Subjects;

namespace Outracks.UnoHost.OSX.FusionSupport
{
	using UnoView.RenderTargets;

	class SurfaceCache
	{
		readonly ConcurrentQueue<SurfaceCacheItem> _surfacesToBeWiped = new ConcurrentQueue<SurfaceCacheItem>(); 
		SurfaceCacheItem _backSurface;
		SurfaceCacheItem _frontSurface;

		readonly Subject<Unit> _surfaceSwapped = new Subject<Unit>();
		public IObservable<Unit> SurfaceSwapped { get { return _surfaceSwapped; } }

		public Optional<TextureInfo> GetCurrentFrontTexture()
		{
			//THIS METHOD MUST BE CALLED IN THE RIGHT GL CONTEXT
			return _frontSurface
				.ToOptional()
				.Select(surface => new TextureInfo(surface.GetOrCreateTexture(), surface.Surface.Size));
		}

		public void SwapAndUpdateCache(int surfaceId)
		{
			var newFrontSurface = GetSurfaceFromCache(surfaceId)
				.Or(() => CreatCacheItem(surfaceId));
			newFrontSurface.Do(Swap);
		}

		public void WipeUnusedSurfaces()
		{
			//THIS METHOD MUST BE CALLED IN THE RIGHT GL CONTEXT
			SurfaceCacheItem item;
			while(_surfacesToBeWiped.TryDequeue(out item))
				item.Dispose();
		}

		void Swap(SurfaceCacheItem newFrontSurface)
		{
			if (GetSurfaceId(newFrontSurface) != GetSurfaceId(_backSurface))
			{
				// If new surface hasn't been cached, then pin the old one for deletion
				if(_backSurface != null)
					PinSurfaceItemForWipe(_backSurface);
			}

			_backSurface = _frontSurface;
			_frontSurface = newFrontSurface;
			_surfaceSwapped.OnNext(Unit.Default);
		}

		void PinSurfaceItemForWipe(SurfaceCacheItem item)
		{
            item.ThrowIfNull("item");
            _surfacesToBeWiped.Enqueue(item);
		}

		Optional<SurfaceCacheItem> GetSurfaceFromCache(int surfaceId)
		{
			if (GetSurfaceId(_backSurface).Or(-1) == surfaceId)
				return _backSurface;

			if (GetSurfaceId(_frontSurface).Or(-1) == surfaceId)
				return _frontSurface;

			return Optional.None();
		}

		static Optional<int> GetSurfaceId(SurfaceCacheItem item)
		{
			if (item == null)
				return Optional.None();

			return item.Surface.GetSurfaceId();
		}

		Optional<SurfaceCacheItem> CreatCacheItem(int surfaceId)
		{
			try
			{
				return new SurfaceCacheItem(IOSurface.CreateFromLookup(surfaceId));
			}
			catch(FailedToLookupSurface)
			{
				return Optional.None();
			}
		}
	}
}
