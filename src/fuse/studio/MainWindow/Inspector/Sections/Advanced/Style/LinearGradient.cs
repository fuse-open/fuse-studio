using System.Reactive.Linq;
using Outracks.Fuse.Studio;
using Outracks.Fusion;

namespace Outracks.Fuse.Inspector.Sections
{
	static class LinearGradientSection
	{
		public static IControl Create(IElement element, IEditorFactory editors)
		{
			return editors.ElementList(
				"Linear Gradient", element,
				SourceFragment.FromString(
					"<LinearGradient>\n" +
					"\t<GradientStop Offset=\"0\" Color=\"#ADDDAB\"/>\n" +
					"\t<GradientStop Offset=\"1\" Color=\"#6DC0D2\"/>\n" +
					"</LinearGradient>\n"),
				linearGradient => CreateGradientRow(linearGradient, editors)
					.WithInspectorPadding());
		}

		static IControl CreateGradientRow(IElement linearGradient, IEditorFactory editors)
		{
			var name = linearGradient.UxName();

			var stops = linearGradient.Children
				.Where(e => e.Is("Fuse.Drawing.GradientStop"));

			return Layout.StackFromTop(
				editors.NameRow("Gradient Name", name),

				Spacer.Medium,

				stops
					.PoolPerElement((index, gradientStop) =>
						CreateGradientStopRow(index, gradientStop.Switch(), editors))
					.StackFromTop(separator: () => Spacer.Small),

				Spacer.Medium,

				Buttons.DefaultButton(
					text:"Remove Gradient Stop",
					cmd: stops
						.Select(s =>
							s.LastOrNone().MatchWith(
								some: stop => Command.Enabled(() => stop.Cut()),
								none: () => Command.Disabled))
						.Switch()),

				Spacer.Small,

				Buttons.DefaultButton(
					text: "Add Gradient Stop",
					cmd: Command.Enabled(() => linearGradient.Paste(SourceFragment.FromString("<GradientStop Offset=\"1\"/>")))));
		}

		static IControl CreateGradientStopRow(int index, IElement gradientStop, IEditorFactory editors)
		{
			var stopName = gradientStop.UxName();
			var stopOffset = gradientStop.GetDouble("Offset", 0.0);
			var stopColor = gradientStop.GetColor("Color", Color.White);

			var colorEditor = editors.Color(stopColor);
			var offsetEditor = editors.Field(stopOffset, deferEdit: true);
			var nameEditor = editors.Field(stopName, placeholderText: "Stop name");

			IControl colorControl = colorEditor;
			IControl offsetControl = offsetEditor;
			IControl nameControl = nameEditor;

			if (index == 0)
			{
				colorControl = colorEditor.WithLabelAbove("Color");
				offsetControl = offsetEditor.WithLabelAbove("Offset");
				nameControl = nameEditor.WithLabelAbove("Name");
			}

			return Layout.Dock()
				.Left(colorControl)
				.Left(Spacer.Small)
				.Left(offsetControl.WithWidth(CellLayout.HalfCellWidth))
				.Left(Spacer.Small)
				.Fill(nameControl);
		}
	}
}
