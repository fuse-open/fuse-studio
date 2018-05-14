using System;
using System.IO;
using System.Linq;
using System.Reactive.Linq;
using System.Threading.Tasks;
using Outracks.Fusion;
using Outracks.IO;

namespace Outracks.Fuse
{
	public static class BrowseButton
	{
		internal static IControl Create(
			IProperty<string> property,
			IObservable<AbsoluteDirectoryPath> projectRoot,
			FileFilter[] fileFilters,
			Text dialogCaption)
		{
			var browseCommand = Command.Enabled(() => Task.Run(
				async () =>
				{
					var root = await projectRoot.FirstAsync();
					var fn = await FileDialog.OpenFile(new FileDialogOptions(await dialogCaption.FirstAsync() ?? "Open", root, fileFilters));
					fn.Do(
						fdr =>
						{
							// Have tested that RelativeTo works on Windows for paths on different
							// drives; in that case it just gives us back the absolute path
							var relPath = fdr.Path.RelativeTo(root).NativeRelativePath;

							// Replace backslash on Windows to make the path valid across platforms
							if (Path.DirectorySeparatorChar == '\\')
								relPath = relPath.Replace('\\', '/');
							property.Write(relPath, true);
						});
				}));

			var arrowBrush = property.IsReadOnly.Select(ro => ro ? Theme.FieldStroke.Brush : Theme.Active).Switch();
			var browseButtonCircles =
				Layout.StackFromLeft(
						Enumerable.Range(0, 3).Select(
							i => Shapes.Circle(fill: arrowBrush)
								.WithSize(new Size<Points>(2, 2))
								.WithPadding(new Thickness<Points>(1))))
					.WithPadding(new Thickness<Points>(3));
			return browseButtonCircles
				.Center()
				.WithWidth(21)
				.WithBackground(Theme.PanelBackground)
				.OnMouse(pressed: browseCommand);
		}
	}
}