using System;
using System.Collections.Generic;
using System.IO;
using System.Reactive.Concurrency;
using System.Reactive.Linq;
using System.Reactive.Subjects;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Media.Imaging;

namespace Outracks.Fusion.Windows
{
	static class ImageImplementation
	{
		static readonly Ratio<Pixels, Points> OneToOnePixelToPoints = new Ratio<Pixels, Points>(1.0);

		// Default to loading last seen dpi image
		static Ratio<Pixels, Points> _lastSeenDpi = new Ratio<Pixels, Points>(1.0);

		public static void Initialize(Dispatcher dispatcher)
		{
			Image.Implementation.Factory = (streams, overlayColor, colorMap, dpiOverrride) => ImageImpl(dispatcher, streams, overlayColor, colorMap, dpiOverrride);
			Image.Implementation.Animate = (images, duration, overlayColor) =>
			{
				var totalSize = new Size<Pixels>();

				return Control.Create(
					self =>
					{
						var imgBrushes = new List<ImageBrush>();

						foreach (var imageStream in images)
						{
							var bitmap = DecodeImage(imageStream.Create());
							totalSize = totalSize.Max(bitmap.Width, bitmap.Height);
							imgBrushes.Add(new ImageBrush(bitmap) { Stretch = Stretch.Uniform });
						}
				
						var timePerImage = duration.TotalMilliseconds / images.Count;
						var imgStream = Observable.Interval(TimeSpan.FromMilliseconds((int) timePerImage))
							.Select(
								f =>
								{
									var index = (int) f % imgBrushes.Count;
									return imgBrushes[index];
								});

						var content = new ContentControl { IsHitTestVisible = false };
						content.GetDpi()
							.ConnectWhile(self.IsRooted)
							.Subscribe(dpi => 
								dispatcher.Schedule(() =>
									content.Content = Rectangle(self, imgStream, totalSize, overlayColor, dispatcher)));

						self.BindNativeDefaults(content, dispatcher);

						return content;
					}).WithSize(totalSize/OneToOnePixelToPoints);
			};
			Image.Implementation.Loader<BitmapImage>.FromStream = DecodeImage;
		}

		static IControl ImageImpl(
			Dispatcher dispatcher,
			IObservable<IImage> imageStreams,
			Optional<IObservable<Color>> overlayColor,
			Optional<IObservable<IColorMap>> colorMap,
			Optional<IObservable<Ratio<Pixels, Points>>> dpiOverride)
		{
			BehaviorSubject<Ratio<Pixels, Points>> controlDpi = new BehaviorSubject<Ratio<Pixels, Points>>(_lastSeenDpi);
			var image = dpiOverride.Or((IObservable<Ratio<Pixels, Points>>)controlDpi)
				.CombineLatest(
					imageStreams,
					colorMap.Select(x => x.Select(y => y.ToOptional())).Or(Observable.Return<Optional<IColorMap>>(Optional.None())),
					(dpi, streams, cmap) =>
					{
						var iv = streams.Load<BitmapImage>(dpi, cmap);
						BitmapImage bitmap = iv.Image;
						return new { bitmap, size = new Size<Pixels>(bitmap.PixelWidth, bitmap.PixelHeight) / iv.ScaleFactor };
					})
				.Replay(1)
				.RefCount();

			return Control.Create(
				self =>
				{
					var dummyControl = new ContentControl { IsHitTestVisible = false };
					self.BindNativeProperty(dispatcher, "dpi", dummyControl.GetDpi(),
						dpi =>
						{
							if (!dpi.Equals(controlDpi.Value))
								controlDpi.OnNext(dpi);
							_lastSeenDpi = dpi;

						});
					dummyControl.Content = Rectangle(
						self,
						image.Select(x => x.bitmap),
						overlayColor,
						dispatcher);
					self.BindNativeDefaults(dummyControl, dispatcher);
					return dummyControl;

				}).WithSize(image.Select(x => x.size).Transpose());
		}


		static FrameworkElement Rectangle(
			IMountLocation model,
			IObservable<ImageBrush> imgStream,
			Size<Pixels> totalSize,
			IObservable<Color> overlayColor,
			IScheduler dispatcher)
		{
			var rect = new System.Windows.Shapes.Rectangle
			{
				MaxWidth = totalSize.Width.Value,
				MaxHeight = totalSize.Height.Value
			};
			if (overlayColor != null)
			{
				imgStream
					.CombineLatest(overlayColor.DistinctUntilChanged())
					.ConnectWhile(model.IsRooted)
					.Subscribe(
						t =>
						{
							dispatcher.Schedule(() =>
							{
								rect.OpacityMask = t.Item1;
								rect.Fill = new SolidColorBrush(t.Item2.ToColor());
							});
						});
			}
			else
			{
				imgStream
					.ConnectWhile(model.IsRooted)
					.Subscribe(
						img =>
						{
							dispatcher.Schedule(() =>
							{
								rect.OpacityMask = img;
							});
						});
			}
			return rect;
		}


		static FrameworkElement Rectangle(
			IMountLocation model,
			IObservable<BitmapImage> imageStream,
			Optional<IObservable<Color>> overlayColor,
			Dispatcher dispatcher)
		{
			return overlayColor.Select(
				color =>
				{
					var rect = new System.Windows.Shapes.Rectangle();

					model.BindNativeProperty(dispatcher, "image", imageStream,
						bitmap =>
						{
							var imgBrush = new ImageBrush(bitmap)
							{
								Stretch = Stretch.Uniform,
								ImageSource = bitmap,
							};
							rect.OpacityMask = imgBrush;
							rect.MaxWidth = imgBrush.ImageSource.Width;
							rect.MaxHeight = imgBrush.ImageSource.Height;
						});

					model.BindNativeProperty(dispatcher, "overlayColor", color, c => rect.Fill = new SolidColorBrush(c.ToColor()));
					return (FrameworkElement)rect;
				}).Or(
				() =>
				{
					var imageControl = new System.Windows.Controls.Image()
					{
						StretchDirection = StretchDirection.DownOnly
					};
					model.BindNativeProperty(dispatcher, "image", imageStream,
						bitmap =>
						{
							imageControl.Source = bitmap;
						});
					RenderOptions.SetBitmapScalingMode(imageControl, BitmapScalingMode.HighQuality);
					return (FrameworkElement)imageControl;
				});
		}

		static BitmapImage DecodeImage(Stream stream)
		{
			var bitmap = new BitmapImage();
			bitmap.BeginInit();
			bitmap.StreamSource = stream;
			bitmap.CacheOption = BitmapCacheOption.OnLoad;
			bitmap.EndInit();
			bitmap.Freeze();
			return bitmap;
		}
	}
}