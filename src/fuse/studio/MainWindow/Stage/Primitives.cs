using System;
using System.Linq;
using System.Reactive.Linq;
using Outracks.Fusion;

namespace Outracks.Fuse.Stage
{
	public static class Primitives
	{
		public static IControl Bar
		{
			get
			{
				var primitiveElements = new[]
				{
					"ClientPanel",
					"Panel",
					"StackPanel",
					"Grid",
					"DockPanel",
					"WrapPanel",
					"Each",
					"ScrollView",
					"Rectangle",
					"Circle",
					"Image",
					"Text",
					"TextInput",
					"Page",
					"PageControl",
					"Button"
				};

				var primitives = primitiveElements.Select(
					elementName => new
					{
						Icon = (Func<IControl>)(() =>  Icons.MediumIcon(elementName)),
						SourceFragment = SourceFragment.FromString(string.Format("<{0} />", elementName)),
						ToolTip = elementName
					});

				return Layout.Dock()
					.Top(Observable.Return(primitives.Select(primitive =>
								Button.Create(Command.Enabled(),
									state =>
										Shapes.Rectangle(
											fill: Observable.Select(
													state.IsHovered,
													(hovering) =>
														hovering
															? Theme.FaintBackground
															: Theme.PanelBackground)
												.Switch()))
								.WithOverlay(primitive.Icon()
										.Center()
										.OnMouse(dragged: Observable.Return(primitive.SourceFragment)))
								.WithWidth(31)
								.WithHeight(31)
								.SetToolTip(primitive.ToolTip)).ToArray())
						.StackFromTop(separator: () => Spacer.Line))
					.Top(Spacer.Line)
					.Bottom(Spacer.Line)
					.Fill(Shapes.Rectangle(fill: Theme.PanelBackground));

			}
		}
	}
}