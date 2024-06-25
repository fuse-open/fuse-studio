using System;
using Outracks.Fusion;
using Outracks.IO;

namespace Outracks.Fuse.Inspector
{
	public interface IEditorFactory
	{
		IControl ElementList(Text name, IElement parent, SourceFragment prototype, Func<IElement, IControl> itemFactory);

		IControl Label(Text name, IProperty<Optional<string>> attributeData);

		IControl Label<T>(Text name, params IAttribute<T>[] properties);

		IControl Label(Text name, IAttribute<Points> attribute);

		IControl Label(Text name, IAttribute<UxSize> attribute);

		IEditorControl Field<T>(IAttribute<T> attribute, Text placeholderText = default(Text), Text toolTip = default(Text), bool deferEdit = false);

		IEditorControl Color(IAttribute<Color> color);

		IEditorControl Switch(IAttribute<bool> attribute);

		IEditorControl Dropdown<T>(IAttribute<T> attribute) where T : struct;

		IEditorControl Slider(IAttribute<double> attribute, double min, double max);

		IEditorControl FilePath(IAttribute<string> attribute, IObservable<AbsoluteDirectoryPath> projectRoot, FileFilter[] fileFilters, Text placeholderText = default(Text), Text toolTip = default(Text));

		IRadioButton<T> RadioButton<T>(IAttribute<T> attribute);

		IControl ExpressionButton<T>(IAttribute<T> attribute);
	}

	public interface IRadioButton<in T>
	{
		IRadioButton<T> Option(T value, Func<Brush, Brush, IControl> icon, Text tooltip);

		IEditorControl Control { get; }
	}

	public interface IEditorControl : IControl
	{
		IControl WithIcon(Text tooltip, IControl icon);

		IControl WithLabel(Text description);

		IControl WithLabelAbove(Text description);
	}
}
