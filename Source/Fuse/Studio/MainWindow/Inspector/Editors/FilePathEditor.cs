using System;
using Outracks.Fusion;
using Outracks.IO;

namespace Outracks.Fuse.Inspector.Editors
{
	public static class FilePathEditor
	{
		public static IControl Create(
			IEditorFactory editors,
			IAttribute<string> property,
			IObservable<AbsoluteDirectoryPath> projectRoot,
			FileFilter[] fileFilters,
			Text placeholderText = default(Text),
			Text toolTip = default(Text),
			Text dialogCaption = default(Text))
		{
			return Layout.Dock()
				.Right(editors.ExpressionButton(property))
				.Fill(FilePathControl.Create(property.StringValue, projectRoot, fileFilters, placeholderText, toolTip, dialogCaption));
		}
	}
}