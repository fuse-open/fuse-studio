using System;
using System.Reactive.Linq;
using Outracks.Fuse.Inspector.Editors;

namespace Outracks.Fuse.Inspector.Sections
{
	using Fusion;
	
	class SpacingSection
	{
		public static IControl Create(IElement element, IEditorFactory editors)
		{
			var margin = element.GetThickness("Margin", new Thickness<Points>(0, 0, 0, 0));
			var padding = element.GetThickness("Padding", new Thickness<Points>(0, 0, 0, 0));

			return Layout.StackFromTop(
					ThicknessEditor(margin, editors).WithLabel("Margin"),
					Spacer.Small,
					ThicknessEditor(padding, editors).WithLabel("Padding"))
				.WithInspectorPadding();
		}

		static IEditorControl ThicknessEditor(IAttribute<Thickness<Points>> thickness, IEditorFactory editors)
		{
			var expressions = Decompose(thickness);
			var cells =
				expressions.Select(
					(prop, edge) =>
						TextBox.Create(
								prop.Convert(UxSizeParser.Serialize, s => s.TryParsePoints().Value.Or(new Points(0))),
								foregroundColor: Theme.DefaultText)
							.WithPadding(new Thickness<Points>(3,0,0,0))
							.WithOverlay(Shapes.Rectangle(fill: Theme.FieldStroke.Brush).WithSize(new Size<Points>(2, 2)).Dock(edge))
							.WithOverlay(Shapes.Rectangle(Theme.FieldStroke))
							.WithBackground(Shapes.Rectangle(fill: Theme.FieldBackground))
							.WithWidth(28)
							.WithHeight(CellLayout.DefaultCellHeight)
							.SetToolTip(edge.ToString()));

			return new EditorControl<Thickness<Points>>(
				editors, thickness,
				Layout.StackFromLeft(
					cells.Left, Spacer.Small, 
					cells.Top, Spacer.Small, 
					cells.Right, Spacer.Small, 
					cells.Bottom, 
					Spacer.Medim, 
					editors.ExpressionButton(thickness).WithPadding(right: new Points(1))));
		}

		static Thickness<IAttribute<Points>> Decompose(IAttribute<Thickness<Points>> thickness)
		{
			return new Thickness<IAttribute<Points>>(
				left: Focus(thickness, t => t.Left, (t, v) => t.With(left: v)),
				top: Focus(thickness, t => t.Top, (t,v) => t.With(top: v)),
				right: Focus(thickness, t => t.Right, (t, v) => t.With(right: v)),
				bottom: Focus(thickness, t => t.Bottom, (t, v) => t.With(bottom: v)));
		}

		private static IAttribute<Points> Focus(
			IAttribute<Thickness<Points>> thickness,
			Func<Thickness<Points>, Points> convert,
			Func<Thickness<Points>, Points, Thickness<Points>> combine)
		{
			return thickness.Focus(
				e => e.Select(convert, t => t.Value.ToString("0.##")),
				convert,
				combine,
				Command.Enabled(() => thickness.Take(1).Subscribe(lastValue => thickness.Write(combine(lastValue, new Points(0))))));
		}
	}
}