using System;
using System.IO;
using System.Linq;
using System.Reactive.Linq;
using System.Threading.Tasks;
using Outracks.Fuse.Inspector;
using Outracks.Fuse.Inspector.Editors;
using Outracks.Fusion;
using Outracks.IO;

namespace Outracks.Fuse
{
	public static class FilePathControl
	{
		internal static IControl Create(
			IProperty<string> property,
			IObservable<AbsoluteDirectoryPath> projectRoot,
			FileFilter[] fileFilters,
			Text placeholderText,
			Text toolTip,
			Text dialogCaption)
		{
			var hidePlaceholder = property.Select(x => !string.IsNullOrEmpty(x));
			return Layout.Dock()
				.Right(BrowseButton.Create(property, projectRoot, fileFilters, dialogCaption))
				.Right(Separator.Line(Theme.FieldStroke))
				.Fill(
					TextBox.Create(
							property,
							foregroundColor: Theme.DefaultText)
						.WithPlaceholderText(hidePlaceholder, placeholderText)
						.SetToolTip(toolTip))
				.WithBorderAndSize();
		}

		static IControl WithBorderAndSize(this IControl control)
		{
			return control
				.WithPadding(new Thickness<Points>(1))
				.WithOverlay(Shapes.Rectangle(stroke: Theme.FieldStroke))
				.WithBackground(Shapes.Rectangle(fill: Theme.FieldBackground))
				.WithHeight(CellLayout.DefaultCellHeight)
				.WithWidth(CellLayout.FullCellWidth);
		}
	}
}