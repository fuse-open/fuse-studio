using System;
using System.Reactive.Linq;

namespace Outracks.Fuse.Inspector.Editors
{
	using Fusion;
	
	static class FieldEditor
	{
		public static IControl Create<T>(
			IEditorFactory editors,
			IAttribute<T> property,
			Optional<Command> onFocused = default(Optional<Command>),
			Text placeholderText = default(Text),
			Text toolTip = default(Text),
			bool deferEdit = false)
		{
			var stringValue = property.StringValue;
			var hasValue = property.HasLocalValue();

			if (deferEdit)
			{
				stringValue = stringValue.Deferred();
				hasValue = stringValue.Select(s => s != "");
			}

			return Layout.Dock()
				.Right(editors.ExpressionButton(property))
				.Fill(TextBox.Create(
						stringValue,
						onFocused: onFocused,
						foregroundColor: Theme.DefaultText)
					.WithPlaceholderText(hasValue, placeholderText)
					.SetToolTip(toolTip).WithBorderAndSize())
				;
		}

		public static IControl Create(
			IEditorFactory editors,
			IProperty<Optional<string>> property,
			Text placeholderText = default(Text),
			bool deferEdit = false)
		{
			property = deferEdit ? property.Deferred() : property;

			var prop = property.OrEmpty();
			return TextBox.Create(prop, foregroundColor: Theme.DefaultText)
				.WithPlaceholderText(property.Select(d => d.HasValue), placeholderText)
				.WithBorderAndSize().CenterVertically();
		}

		public static IControl WithPlaceholderText(this IControl control, IObservable<bool> hasValue, Text placeholderText = default(Text))
		{
			if (!placeholderText.IsDefault)
				control = control.WithOverlay(
					Label.Create(placeholderText.AsText(), Theme.DefaultFont, color: Theme.DescriptorText)
						.CenterVertically()
						.HideWhen(hasValue)
						.WithPadding(left: new Points(3)));

			return control.WithPadding(left: new Points(3));
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