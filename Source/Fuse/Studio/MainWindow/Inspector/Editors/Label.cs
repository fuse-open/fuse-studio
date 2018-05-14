using System;
using System.Reactive.Linq;

namespace Outracks.Fuse.Inspector.Editors
{
	using Fusion;

	class LabelEditor
	{
		public static IControl Create<T>(Text text, params IAttribute<T>[] properties)
		{
			var hasValue = Observable.Return(false);
			var isReadOnly = Observable.Return(false);
			foreach (var property in properties)
			{
				hasValue = hasValue.Or(property.HasLocalValue());
				isReadOnly = isReadOnly.Or(property.IsReadOnly);
			}
			return Create(text, hasValue, isReadOnly);
		}

		public static IControl Create(Text text, IObservable<bool> hasValue, IObservable<bool> isReadOnly, bool isHittable = false)
		{
			return Fusion.Label
				.Create(
					text: text, 
					font: Theme.DefaultFont, 
					textAlignment: TextAlignment.Left,
					color: isReadOnly.Select(r => !r ? Theme.DefaultText : Theme.DisabledText).Switch())
				.CenterVertically()
				.WithHeight(CellLayout.DefaultCellHeight);
		}

		public static IControl Create<T>(Text text, IAttribute<T> property, IObservable<bool> hasValue = null)
		{
			return Create(text, hasValue ?? property.HasLocalValue(), property.IsReadOnly, property.Clear, Property.Default<Points>());
		}

		public static IControl Create(Text text, IAttribute<Points> property, IObservable<bool> hasValue = null)
		{
			return Create(text, hasValue ?? property.HasLocalValue(), property.IsReadOnly, property.Clear, property);
		}

		public static IControl Create(Text text, IAttribute<UxSize> property)
		{
			var propertyInPoints = property.Focus<UxSize, Points>(s =>
					s.PointsValue.Select(p => p.ToDouble()).Or(() =>
					s.PixelsValue.Select(p => p.ToDouble()).Or(() =>
					s.PercentagesValue.Value.ToDouble())),
				(s, vv) =>
					s.PointsValue.HasValue
						? UxSize.Points(vv.ToDouble())
						: s.PixelsValue.HasValue
							? UxSize.Pixels(vv.ToDouble())
							: UxSize.Percentages(vv.ToDouble()),
				UxSizeParser.TryParsePoints,
				UxSizeParser.Serialize,
				0.0);

			return Create(text, property.HasLocalValue(), property.IsReadOnly, property.Clear, propertyInPoints);
		}

		public static IControl Create(
			Text text,
			IObservable<bool> hasValue,
			IObservable<bool> isDisabled,
			Command clear,
			IProperty<Points> scrub = null)
		{
			var label = Fusion.Label
				.Create(
					text: text,
					font: Theme.DefaultFont,
					textAlignment: TextAlignment.Left,
					color: isDisabled.Select(r => !r ? Theme.DefaultText : Theme.DisabledText).Switch())
				.CenterVertically()
				.WithHeight(CellLayout.DefaultCellHeight)
				;

			if (scrub != null)
				label = label
					.WhileDraggingScrub(scrub)
					.SetCursor(isDisabled.Select(d => d ? Cursor.Normal : Cursor.ResizeHorizontally));

			return label
				.SetContextMenu(Menu.Item("Clear", clear));
		}
	}
}