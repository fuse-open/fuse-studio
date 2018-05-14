using System;
using System.Linq;
using System.Reactive.Linq;

namespace Outracks.Fuse.Inspector.Editors
{
	using Fusion;

	static class ExpressionEditor
	{
		public static IControl CreateButton<T>(IObservable<object> elementChanged, IAttribute<T> property, IPopover popover)
		{
			return popover.CreatePopover(
					RectangleEdge.Bottom,
					content: state =>
						Button.Create(clicked: state.IsVisible.Toggle(), content: button =>
						{
							var circleColor = Observable.CombineLatest(
									button.IsHovered,
									state.IsVisible,
									(hovering, visible) =>
										visible
											? Theme.Active
											: (hovering ? Theme.Active : Theme.FieldFocusStroke.Brush))
								.Switch();

							return Layout.StackFromTop(
									Enumerable.Range(0, 3).Select(
										i => Shapes.Circle(fill: circleColor)
											.WithSize(new Size<Points>(2, 2))
											.WithPadding(new Thickness<Points>(1))))
								.WithPadding(new Thickness<Points>(3));
						}),
					popover: state =>
					{
						var result = Layout.Dock()
							.Bottom(Button.Create(state.IsVisible.Update(false), bs =>
								Layout.Dock()
									.Left(Icons.Confirm(Theme.Active).CenterVertically())
									.Left(Spacer.Small)
									.Fill(Theme.Header("Done"))
									.Center()
									.WithHeight(30)))
							.Bottom(Separator.Medium)
							.Top(Spacer.Medium)
							.Top(Label.Create(
								text: "Expression Editor",
								textAlignment: TextAlignment.Center,
								font: Theme.DefaultFont,
								color: Theme.DefaultText))
							.Top(Spacer.Small)
							.Top(Label.Create(
								text: "You can write expressions here instead \n of using an explicit value",
								textAlignment: TextAlignment.Center,
								font: Theme.DescriptorFont,
								color: Theme.DescriptorText))
							.Top(Spacer.Medium)
							.Left(Spacer.Medium)
							.Right(Spacer.Medium)
							.Bottom(Spacer.Medium)
							.Fill(
								TextBox.Create(
									text: property.StringValue.Deferred(),
									foregroundColor: Theme.DefaultText,
									doWrap: true)
								.WithPadding(new Thickness<Points>(1))
								.WithBackground(Theme.FieldBackground)
								.WithOverlay(Shapes.Rectangle(stroke: Theme.FieldStroke))
								.WithHeight(74))
							.WithWidth(279);

						elementChanged.ConnectWhile(result.IsRooted).Subscribe(id => state.IsVisible.OnNext(false));

						return result;
					})
				.CenterVertically();
		}
	}
}