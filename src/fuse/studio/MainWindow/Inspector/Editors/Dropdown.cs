using System;
using System.Collections.Generic;
using System.Linq;
using System.Reactive.Linq;
using Outracks.Fuse.Studio;
using Outracks.Fusion;

namespace Outracks.Fuse.Inspector.Editors
{
	public class DropdownEditor
	{
		public static IControl Create<T>(IAttribute<T> property, IEditorFactory editors)
			where T : struct
		{
			var objectProperty = property.Convert(t => (object)t, s => (s is T ? (T)s : default(T)));
			var objectValues = Observable.Return(Enum.GetValues(typeof(T)).OfType<object>());
			var placeholderText = property.Select(v => v.ToString()).AsText();
			return Create(property, objectProperty, objectValues, editors, placeholderText);
		}

		public static IControl Create(
			IAttribute<string> property,
			IObservable<string[]> values,
			IEditorFactory editors,
			Text placeholderText = default(Text))
		{
			var objectProperty = property.Convert(p => (object) p, o => (o as string) ?? "");
			var objectValues = (IObservable<IEnumerable<object>>)values;
			return Create(property, objectProperty, objectValues, editors, placeholderText);
		}

		static IControl Create<T>(
			IAttribute<T> attribute,
			IProperty<object> objectValue,
			IObservable<IEnumerable<object>> objectValues,
			IEditorFactory editors,
			Text placeholderText = default(Text))
		{
			var stroke = Theme.FieldStroke;
			var arrowBrush = attribute.IsReadOnly.Select(ro => ro ? Theme.FieldStroke.Brush : Theme.Link).Switch();
			return Layout.Dock()
				.Right(editors.ExpressionButton(attribute))
				.Fill(DropDown.Create(objectValue, objectValues, nativeLook: false)
					.WithOverlay(
						Layout.Dock()
							.Right(Arrow
								.WithoutShaft(RectangleEdge.Bottom, SymbolSize.Small, arrowBrush)
								.Center().WithWidth(21)
								.WithBackground(Theme.PanelBackground))
							.Right(Separator.Line(stroke))
							.Fill(TextBox.Create(
									attribute.StringValue,
									foregroundColor: Theme.DefaultText)
								.WithPlaceholderText(attribute.HasLocalValue(), placeholderText)
								.WithBackground(Theme.FieldBackground))
							.WithPadding(new Thickness<Points>(1))
							.WithOverlay(Shapes.Rectangle(stroke: stroke)))
						.WithHeight(CellLayout.DefaultCellHeight)
						.WithWidth(CellLayout.FullCellWidth));
		}
	}
}
