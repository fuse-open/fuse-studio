using System;
using System.Collections.Generic;
using System.Linq;
using System.Reactive.Linq;
using Outracks.Fusion;

namespace Outracks.Fuse.Inspector.Editors
{
	class RadioButtonOption<T>
	{
		public T Value;
		public Text Tooltip;
		public Func<Brush, Brush, IControl> Icon;
	}

	public class RadioButtonCellBuilder<T> : IRadioButton<T>
	{
		readonly IEditorFactory _editors;
		readonly IAttribute<T> _property;
		readonly List<RadioButtonOption<T>> _options = new List<RadioButtonOption<T>>();
		public RadioButtonCellBuilder(IAttribute<T> property, IEditorFactory editors)
		{
			_property = property;
			_editors = editors;
		}

		public IRadioButton<T> Option(
			T value,
			Func<Brush, Brush, IControl> icon,
			Text tooltip)
		{
			_options.Add(new RadioButtonOption<T>()
			{
				Value = value,
				Icon = icon,
				Tooltip = tooltip,
			});

			return this;
		}

		public IEditorControl Control
		{
			get
			{
				return new EditorControl<T>(_editors, _property,
					Observable.Return(
						_options.Select((v, i) =>
							Button.Create(
								Command.Enabled(() => _property.Write(v.Value, save: true)),
								state =>
								{
									var colors = FindColors(
										isEnabled: state.IsEnabled,
										isDefaultValue: _property.Select(lv => lv.Equals(v.Value)),
										isSetValue: _property.LocalValue().Select(lv => lv.Equals(v.Value)),
										isHovered: state.IsHovered);

									var stroke = Stroke.Create(1, colors.Stroke);

									return v.Icon(colors.Foreground, colors.Background)
										.WithPadding(left: new Points(7), right: new Points(7))
										.Center()
										.WithBackground(
											i == 0 || i == _options.Count - 1
												? Shapes.Rectangle(
													stroke: stroke,
													fill: colors.Background,
													cornerRadius: Observable.Return(new CornerRadius(4)))
													.Scissor(i == 0 ? RectangleEdge.Right : RectangleEdge.Left, 4)
												: Layout.Dock()
													.Top(Separator.Line(stroke))
													.Bottom(Separator.Line(stroke))
													.Fill(Shapes.Rectangle(fill: colors.Background)));
								},
								toolTip: v.Tooltip)))
					.StackFromLeft(separator: () => Separator.Line(Stroke.Create(1, Theme.LineBrush)))
					.WithHeight(CellLayout.DefaultCellHeight));

			}
		}

		Colors FindColors(IObservable<bool> isEnabled, IObservable<bool> isDefaultValue, IObservable<bool> isSetValue, IObservable<bool> isHovered)
		{
			var tmp = Observable.CombineLatest(isEnabled, isDefaultValue, isSetValue, isHovered, FindColors);

			return new Colors
			{
				Background = tmp.Select(t => t.Background).Switch(),
				Foreground = tmp.Select(t => t.Foreground).Switch(),
				Stroke = tmp.Select(t => t.Stroke).Switch(),
			};
		}

		Colors FindColors(bool isEnabled, bool isDefaultValue, bool isSetValue, bool isHovered)
		{
			if (isEnabled)
			{
				if (isSetValue)
					return new Colors
					{
						Background = Theme.Link,
						Foreground = Color.White,
						Stroke = Theme.Link,
					};

				if (isDefaultValue)
					return new Colors
					{
						Background = Theme.LineBrush,
						Foreground = isHovered ? Theme.Link : Theme.DefaultText,
						Stroke = Theme.LineBrush,
					};

				return new Colors
				{
					Background = Theme.PanelBackground,
					Foreground = isHovered ? Theme.Link : Theme.DefaultText,
					Stroke = Theme.LineBrush,
				};
			}
			else
			{
				return new Colors
				{
					Background = Theme.PanelBackground,
					Foreground = Theme.DisabledText,
					Stroke = Theme.DisabledText,
				};
			}
		}

		struct Colors
		{
			public Brush Background;
			public Brush Foreground;
			public Brush Stroke;
		}
	}


}
