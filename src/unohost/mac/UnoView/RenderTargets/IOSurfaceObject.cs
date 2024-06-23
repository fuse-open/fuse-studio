using System;
using System.Runtime.InteropServices;
using Foundation;
using ObjCRuntime;

namespace Outracks.UnoHost.Mac.UnoView.RenderTargets
{
	class FailedToLookupSurface : Exception
	{
		public FailedToLookupSurface(int surfaceId) : base("Failed to lookup an IOSurfaceObject with ID: " + surfaceId)
		{}
	}

	class IOSurfaceObject : IDisposable, INativeObject
	{
		public readonly Size<Pixels> Size;
		readonly IntPtr _ioSurfaceHandle;
		bool _isDisposed;

		public static IOSurfaceObject Create(Size<Pixels> size)
		{
			var width = (int) size.Width;
			var height = (int) size.Height;
			if (width == 0 || height == 0)
				throw new ArgumentOutOfRangeException("size", "IOSurfaceObject dimensions must be greater or equal to 1");

			using(var properties = NSDictionary.FromObjectsAndKeys(
				new object[] {
					width, // IOSurfaceWidth
					height, // IOSurfaceHeight
					4, // IOSurfaceBytesPerElement
					true // IOSurfaceIsGlobal
				},
				new object[] {
					"IOSurfaceWidth",
					"IOSurfaceHeight",
					"IOSurfaceBytesPerElement",
					"IOSurfaceIsGlobal"
				}))
				{
					var surface = new IOSurfaceObject(IOSurfaceCreate(properties.Handle));
					return surface;
				}
		}

		/// <exception cref="FailedToLookupSurface"></exception>
		public static IOSurfaceObject CreateFromLookup(int surfaceId)
		{
			var surface = IOSurfaceLookup(surfaceId);
			if(surface == IntPtr.Zero)
				throw new FailedToLookupSurface(surfaceId);

			return new IOSurfaceObject(surface);
		}

		public IntPtr Handle { get { return _ioSurfaceHandle; } }

		IOSurfaceObject(IntPtr ioSurfaceHandle)
		{
			_ioSurfaceHandle = ioSurfaceHandle;
			Size = new Size<Pixels>(IOSurfaceGetWidth(ioSurfaceHandle), IOSurfaceGetHeight(ioSurfaceHandle));
		}

		~IOSurfaceObject()
		{
			Dispose(false);
		}

		public void Dispose()
		{
			Dispose(true);
			GC.SuppressFinalize(this);
		}

		public void Dispose(bool disposing)
		{
			if (_isDisposed)
				return;

			CFRelease(_ioSurfaceHandle);
			_isDisposed = true;
		}

		public int GetSurfaceId()
		{
			return IOSurfaceGetID(_ioSurfaceHandle);
		}

		const string CoreFoundationLibrary = "/System/Library/Frameworks/CoreFoundation.framework/Versions/A/CoreFoundation";
		const string IOSurfaceLibrary = "/System/Library/Frameworks/IOSurface.framework/Versions/A/IOSurface";

		[DllImport(IOSurfaceLibrary)]
		static extern IntPtr IOSurfaceCreate(IntPtr dictionaryRefProperties);

		[DllImport(IOSurfaceLibrary)]
		static extern IntPtr IOSurfaceLookup(int surfaceId);

		[DllImport(IOSurfaceLibrary)]
		static extern int IOSurfaceGetWidth(IntPtr surfaceRef);

		[DllImport(IOSurfaceLibrary)]
		static extern int IOSurfaceGetHeight(IntPtr surfaceRef);

		[DllImport(IOSurfaceLibrary)]
		static extern int IOSurfaceGetID(IntPtr surfaceRef);

		[DllImport(CoreFoundationLibrary)]
		static extern void CFRelease(IntPtr cf);
	}
}
