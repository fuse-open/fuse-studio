using System;
using System.Collections.Generic;
using System.Linq;
using System.Reactive;
using System.Reactive.Concurrency;
using System.Reactive.Linq;
using System.Reactive.Subjects;
using System.Runtime.InteropServices;
using AppKit;
using CoreGraphics;
using Foundation;

namespace Outracks.Fusion.OSX
{
	class TintedImage : NSDefaultView
	{
		Optional<NSImage> _currentImage;
		readonly NSImageView _imageView;

		Optional<NSColor> _tintColor = Optional.None();
		readonly bool _cloneOnTint;

		public Optional<NSColor> TintColor
		{
			get { return _tintColor; }
			set
			{
				_tintColor = value;
				NeedsDisplay = true;
			}
		}

		public Optional<NSImage> CurrentImage
		{
			get { return _currentImage; }
			set
			{
				if (_currentImage == value)
					return;

				_currentImage = value;
				_currentImage.Do(image => _imageView.Image = image);
				NeedsDisplay = true;
			}
		}

		public TintedImage(IntPtr handle) : base(handle)
		{
		}

		public TintedImage(Optional<NSImage> nsImage = default(Optional<NSImage>), bool cloneOnTint = true)
		{
			_cloneOnTint = cloneOnTint;

			_imageView = new NSImageView()
			{
				ImageScaling = NSImageScale.ProportionallyDown
			};

			AddSubview(_imageView);
			nsImage.Do(img => CurrentImage = img);
		}

		public override void DrawRect(CGRect dirtyRect)
		{
			_imageView.Frame = Bounds;

			_tintColor.Do(tintColor => _currentImage.Do(
				currentImage =>
				{
					var image = _cloneOnTint ? new NSImage(currentImage.CGImage.Clone(), currentImage.Size) : currentImage;
					image.LockFocus();
					tintColor.SetFill();
					RectFillUsingOperation(new CGRect(0, 0, image.Size.Width, image.Size.Height), NSCompositingOperation.SourceIn);
					image.UnlockFocus();
					if (_cloneOnTint) _imageView.Image = image;
				}));

			base.DrawRect(dirtyRect);
		}

		[DllImport("/System/Library/Frameworks/AppKit.framework/AppKit", EntryPoint = "NSRectFillUsingOperation")]
		public static extern void RectFillUsingOperation(CGRect rect, NSCompositingOperation operation);
	}

	class DpiAwareView : NSView
	{
		readonly ReplaySubject<Ratio<Pixels, Points>> _density = new ReplaySubject<Ratio<Pixels, Points>>(1);

		public DpiAwareView() { }

		public DpiAwareView(IntPtr handle) : base(handle) { }

		[Export("viewDidChangeBackingProperties")]
		public void ViewDidChangeBackingProperties()
		{
			_density.OnNext(GetDensity());
		}

		Ratio<Pixels, Points> GetDensity()
		{
			return new Ratio<Pixels, Points>(ConvertSizeToBacking(new CGSize(1, 1)).Width);
		}

		public IObservable<Ratio<Pixels, Points>> GetDpi()
		{
			return _density.DistinctUntilChanged();
		}
	}

	static class ImageImplementation
	{

		public static void Initialize(IScheduler dispatcher)
		{
			Image.Implementation.Factory = (streams, overlayColor, colorMap, dpiOverride) => ImageImpl(dispatcher, streams, overlayColor, colorMap, dpiOverride);
			Image.Implementation.Animate = 
				(imageStreams, duration, overlayColor) => AnimateImpl(imageStreams, duration, overlayColor, dispatcher);
			Image.Implementation.Loader<NSImage>.FromStream = NSImage.FromStream;
		}

		static IControl ImageImpl(
			IScheduler dispatcher,
			IObservable<IImage> streams,
			Optional<IObservable<Color>> overlayColor,
			Optional<IObservable<IColorMap>> colorMap,
			Optional<IObservable<Ratio<Pixels, Points>>> dpiOverride)
		{
			BehaviorSubject<Size<Points>> desiredSize = new BehaviorSubject<Size<Points>>(Size.Zero<Points>());
			return Control.Create(
				ctrl =>
				{
					var dummyControl = new DpiAwareView()
					{
						AutoresizesSubviews = true
					};

					ctrl.BindNativeDefaults(dummyControl, dispatcher);

					var tintedImage = new TintedImage()
					{
						AutoresizingMask = NSViewResizingMask.WidthSizable | NSViewResizingMask.HeightSizable
					};
					dummyControl.AddSubview(tintedImage);

					ctrl.BindNativeProperty(dispatcher, "image", dpiOverride.Or(dummyControl.GetDpi())
							.CombineLatest(streams, colorMap.Select(x => x.Select(Optional.Some)).Or(Observable.Return<Optional<IColorMap>>(Optional.None())),
							(dpi, imgStreams, cm) => new {dpi, imgStreams, colorMap = cm}),
						x =>
						{
							var iv = x.imgStreams.Load<NSImage>(x.dpi, x.colorMap);
							tintedImage.CurrentImage = iv.Image;
							tintedImage.SetFrameSize(dummyControl.Frame.Size);
							var size = Size.Create<Pixels>((double)iv.Image.Size.Width, (double)iv.Image.Size.Height) / iv.ScaleFactor;
							desiredSize.OnNext(size);
						});

					overlayColor.Do(
						colorObs =>
						{
							ctrl.BindNativeProperty(
								dispatcher,
								"tintColor",
								colorObs,
								color => tintedImage.TintColor = color.ToNSColor());

						});

					return dummyControl;
				}).WithSize(desiredSize.Transpose());
		}

		static IControl AnimateImpl(IList<ImageStream> streams, TimeSpan duration, IObservable<Color> overlayColor, IScheduler dispatcher)
		{
			var imgSize = new Size<Points>(0, 0);

			return Control.Create(
					ctrl =>
					{
						var nsimages = new List<NSImage>();

						foreach (var imageStream in streams)
						{
							var img = NSImage.FromStream(imageStream.Create());	
							imgSize = imgSize.Max((double)img.Size.Width, (double)img.Size.Height);
							nsimages.Add(img);
						}
						
						var timePerImage = duration.TotalMilliseconds / nsimages.Count;
						var images = Observable.Interval(TimeSpan.FromMilliseconds((int)timePerImage))
							.Select(f => nsimages[(int)f % nsimages.Count]);
						
						var dummyControl = new DpiAwareView()
						{
							AutoresizesSubviews = true
						};

						dummyControl
							.GetDpi()
							.ConnectWhile(ctrl.IsRooted)
							.Select(
								dpi => Observable.FromAsync(
									async () =>
									{
										var tintedImage = await Fusion.Application.MainThread.InvokeAsync(
											() =>
												new TintedImage(nsimages[0], false)
												{
													AutoresizingMask = NSViewResizingMask.WidthSizable | NSViewResizingMask.HeightSizable
												});

										ctrl.BindNativeProperty(
											dispatcher,
											"currImage",
											images,
											image => tintedImage.CurrentImage = image);
										ctrl.BindNativeProperty(
											dispatcher,
											"tintColor",
											overlayColor,
											color => tintedImage.TintColor = color.ToNSColor());


										await Fusion.Application.MainThread.InvokeAsync(
											() =>
											{
												if (dummyControl.Subviews.IsEmpty() == false)
													dummyControl.Subviews.Last().RemoveFromSuperview();

												tintedImage.SetFrameSize(dummyControl.Frame.Size);
												dummyControl.AddSubview(tintedImage);

												return Unit.Default;
											});

										return Unit.Default;
									}))
							.Concat()
							.Subscribe();

						ctrl.BindNativeDefaults(dummyControl, dispatcher);

						return dummyControl;
					}).WithSize(imgSize);
			}

	}
}
