using System;
using System.Reactive.Linq;
using Outracks.Fusion;
using Outracks.IO;

namespace Outracks.Fuse.Inspector.Editors
{
	public class Factory : IEditorFactory
	{
		readonly IObservable<object> _elementChanged;
		readonly IPopover _popover;
		public Factory(IObservable<object> elementChanged, IPopover popover)
		{
			_elementChanged = elementChanged;
			_popover = popover;
		}

		public IControl ElementList(Text name, IElement parent, SourceFragment prototype, Func<IElement, IControl> itemFactory)
		{
			return ListEditor.Create(parent, name, prototype, itemFactory);
		}

		public IControl Label<T>(Text name, params IAttribute<T>[] properties)
		{
			return LabelEditor.Create(name, properties);
		}

		public IControl Label(Text name, IAttribute<Points> attribute)
		{
			return LabelEditor.Create(name, attribute);
		}

		public IControl Label(Text name, IAttribute<UxSize> attribute)
		{
			return LabelEditor.Create(name, attribute);
		}

		public IControl Label(Text name, IProperty<Optional<string>> attributeData)
		{
			return LabelEditor.Create(name, attributeData.Select(e => e.HasValue), attributeData.IsReadOnly, Command.Enabled(() => attributeData.Write(Optional.None())));
		}

		public IEditorControl Field<T>(IAttribute<T> attribute, Text placeholderText = default(Text), Text toolTip = default(Text), bool deferEdit = false)
		{
			return Wrap(attribute, FieldEditor.Create(this, attribute, placeholderText: placeholderText, toolTip: toolTip, deferEdit: deferEdit));
		}

		public IEditorControl Switch(IAttribute<bool> attribute)
		{
			return Wrap(attribute, Layout.StackFromLeft(
				SwitchEditor.Create(attribute)
					.CenterVertically(),
				Spacer.Medim,
				ExpressionButton(attribute).WithPadding(right: new Points(1))
					.CenterVertically()));
		}

		public IEditorControl Color(IAttribute<Color> color)
		{
			return Wrap(color, ColorEditor.Create(color, this));
		}
		public IEditorControl Slider(IAttribute<double> attribute, double min, double max)
		{
			return Wrap(attribute, SliderEditor.Create(attribute, min, max));
		}

		public IEditorControl FilePath(IAttribute<string> attribute, IObservable<AbsoluteDirectoryPath> projectRoot, FileFilter[] fileFilters, Text placeholderText = default(Text), Text toolTip = default(Text))
		{
			return Wrap(attribute, FilePathEditor.Create(this, attribute, projectRoot, fileFilters, placeholderText: placeholderText, toolTip: toolTip));
		}

		public IEditorControl Dropdown<T>(IAttribute<T> attribute) where T : struct
		{
			return Wrap(attribute, DropdownEditor.Create(attribute, this));
		}

		IEditorControl Wrap<T>(IAttribute<T> property, IControl control)
		{
			return new EditorControl<T>(this, property, control);
		}

		public IRadioButton<T> RadioButton<T>(IAttribute<T> attribute)
		{
			return new RadioButtonCellBuilder<T>(attribute, this);
		}

		public IControl ExpressionButton<T>(IAttribute<T> attribute)
		{
			return ExpressionEditor.CreateButton(_elementChanged, attribute, _popover);
		}
	}
}
