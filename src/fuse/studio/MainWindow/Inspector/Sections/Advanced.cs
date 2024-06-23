using System;
using System.Reactive.Linq;
using Outracks.Fuse.Studio;
using Outracks.Fusion;

namespace Outracks.Fuse.Inspector.Sections
{
	class AdvancedSection
	{
		public static IControl Create(IElement element, IEditorFactory editors)
		{
			return Layout.StackFromTop(
				Separator.Shadow,
				Separator.Weak,
				Pane("Layout", editors, e => LayoutSection.Create(element, e)),
				Pane("Size / Position", editors, e => SizePositionSection.Create(element, e)),
				Pane("Style", editors, e => StyleSection.Create(element, e)),
				Pane("Transform", editors, e => TransformSection.Create(element, e)),
				Pane("Visibility", editors, e => VisibilitySection.Create(element, e)),
				Pane("Attributes", editors, e => AttributesSection.Create(element, e), collapse: element.SimulatorId));
		}

		static IControl Pane(string name, IEditorFactory editorFactory, Func<IEditorFactory, IControl> content, IObservable<object> collapse)
		{
			return Pane(name, editorFactory, content, lazy: true,
				// Create a new backing store for expanded value each time collapse emits a value
				expanded: collapse
					.Select(_ => Property.Create(false))
					.Switch());
		}

		static IControl Pane(string name, IEditorFactory editorFactory, Func<IEditorFactory, IControl> content)
		{
			return Pane(name, editorFactory, content, lazy: false,
				// Store expanded state as "Expand <name>" : "true", remove from settings when false
				expanded: UserSettings
					.Bool("Expand " + name)
					.OrFalse());
		}

		static IControl Pane(string name, IEditorFactory editorFactory, Func<IEditorFactory, IControl> content, bool lazy, IProperty<bool> expanded)
		{
			var attributes = new AttributeIntercepter(editorFactory);

			var labelColor = lazy
				? Theme.DefaultText
				: attributes.AllReadOnly
					.Select(r => r ? Theme.DisabledText : Theme.DefaultText)
					.Switch();

			var height = new Points(31);

			var label = Label.Create(name, font: Theme.DefaultFontBold, color: labelColor)
				.CenterVertically();

			var expandedArrow = Arrow.WithoutShaft(RectangleEdge.Bottom, SymbolSize.Small);
			var collapsedArrow = Arrow.WithoutShaft(RectangleEdge.Right, SymbolSize.Small);

			var arrow = expanded
				.Select(expanded1 => expanded1 ? expandedArrow : collapsedArrow)
				.Switch()
				.Center()
				.WithSize(Size.Create<Points>(13, 13))
				.OnMouse(pressed: expanded.Toggle())
				.CenterVertically();

			var circle = Shapes.Circle(fill: Theme.Link)
				.WithSize(Spacer.Small.DesiredSize)
				.CenterVertically()
				.ShowWhen(attributes.AnyHasValue);

			return Layout.StackFromTop(
				Button.Create(
					clicked: expanded.Toggle(),
					content: s =>
						Layout.Dock()
							.Left(label)
							.Left(Spacer.Small)
							.Left(
								circle
									.WithHeight(height)
									.MakeCollapsable(RectangleEdge.Top, expanded.IsFalse(), lazy: false)
									.DockBottom())
							.Right(arrow)
							.Fill()
							.WithHeight(height)
							.WithInspectorPadding()),
				Separator.Medium,
				Layout.StackFromTop(
					content(attributes),
					Separator.Medium)
					.MakeCollapsable(RectangleEdge.Bottom, expanded, lazy: lazy, unrootWhenCollapsed: lazy));
		}
	}
}
