using System;
using System.IO;
using System.Reactive;
using System.Reactive.Linq;
using System.Reactive.Subjects;
using System.Text;
using Outracks.Fuse.Designer;
using Outracks.Fuse.Theming.Themes;
using Outracks.Fusion;
using Outracks.IO;
using Uno.IO;

namespace Outracks.Fuse.Testing
{
	public class IconPreviewWindow
	{
		// 
		static readonly byte[] FallbackImage = Encoding.UTF8.GetBytes(
			"<svg xmlns=\"http://www.w3.org/2000/svg\" width=\"8\" height=\"8\" viewBox=\"0 0 8 8\">\r\n  <path d=\"M0 0v8h8v-8h-8zm1 1h6v3l-1-1-1 1 2 2v1h-1l-4-4-1 1v-3z\" />\r\n</svg>");

		public static void Create()
		{
			var lightTheme = new OriginalLightTheme();
			var darkTheme = new OriginalDarkTheme();

			var path = new BehaviorSubject<string>(string.Empty);
			var refresh = new BehaviorSubject<Unit>(Unit.Default);
			var image = path
					.CombineLatest(refresh, ((p, _) => p))
					.Select(p => AbsoluteFilePath.TryParse(p)
					.Where(f => File.Exists(f.NativePath))
					.Select(
						absPath => absPath.NativePath.EndsWith(".svg")
							? (IImage) new SvgImage(() => File.OpenRead(absPath.NativePath))
							: new MultiResolutionImage(
								new[] { new ImageStream(new Ratio<Pixels, Points>(1), () => File.OpenRead(absPath.NativePath)) }))
					.Or(() => (IImage)new SvgImage(() => new MemoryStream(FallbackImage))));

			var content =
				Layout.Dock().Top(
					Layout.Dock()
						.Left(Label.Create("Path: ", font: Theme.DefaultFont, color: Theme.DefaultText).CenterVertically())
						.Right(Buttons.DefaultButton("Refresh", Command.Enabled(() => refresh.OnNext(Unit.Default))))
						.Fill(ThemedTextBox.Create(path.AsProperty())))
				.Fill(Layout.SubdivideHorizontally(ImageVersionsRowForTheme(image, darkTheme), ImageVersionsRowForTheme(image, lightTheme)))
				.WithBackground(Theme.PanelBackground);

			Application.Desktop.CreateSingletonWindow(
				Observable.Return(true),
				dialog => new Window
				{
					Title = Observable.Return("Icon preview"),
					Size = Property.Create<Optional<Size<Points>>>(new Size<Points>(600, 600)).ToOptional(),
					Content = content,
					Background = Theme.PanelBackground,
					Foreground = Theme.DefaultText,
					Border = Separator.MediumStroke
				});
		}

		static IControl ImageVersionsRowForTheme(IObservable<IImage> image, ITheme theme)
		{
			return Layout.SubdivideVertically(
					Image.FromImage(
						image,
						dpiOverride: Observable.Return(new Ratio<Pixels, Points>(1)),
						colormMap: theme.IconColorMap),
					Image.FromImage(
						image,
						dpiOverride: Observable.Return(new Ratio<Pixels, Points>(2)),
						colormMap: theme.IconColorMap))
				.WithBackground(theme.PanelBackground);
		}
	}
}