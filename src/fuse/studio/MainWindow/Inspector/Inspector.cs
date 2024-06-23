using System;
using System.Reactive.Linq;
using Outracks.Fuse.Inspector.Editors;
using Outracks.Fuse.Inspector.Sections;
using Outracks.Fusion;
using Outracks.Simulator;

namespace Outracks.Fuse.Inspector
{
	public class Inspector
	{
		public static readonly string Title = "Inspector";
		public static readonly Points Width = 295;

		public static IControl Create(IProject project)
		{
			var element = project.Context.CurrentSelection;

			var nothingSelected = element.SimulatorId.Select(id => id == ObjectIdentifier.None);

			var elementChanged = element.SimulatorId.Select(id => (object)id);


			return Popover.Host(popover =>
			{
				var uneditableElementMessage = UneditableElementMessage(project);
				var uneditableElementIsSelected = uneditableElementMessage.Select(x => x.HasValue);
				var uneditablePlaceholder = UneditablePlaceholder(uneditableElementMessage).ShowWhen(uneditableElementIsSelected);

				return Layout.StackFromTop(
						CommonSection.Create(element, project, new Factory(elementChanged, popover), popover),
						AdvancedSection.Create(element, new Factory(elementChanged, popover))
							.MakeCollapsable(RectangleEdge.Top, uneditableElementIsSelected.IsFalse(), animate: false))
					.WithWidth(Width)
					.DockLeft()
					.MakeScrollable(darkTheme: Theme.IsDark, horizontalScrollBarVisible: false)
					.WithBackground(uneditablePlaceholder.ShowWhen(uneditableElementIsSelected))
					.WithOverlay(Placeholder().ShowWhen(nothingSelected));
			});
		}

		static IControl Placeholder()
		{
			Points rectangleWidth = 100;
			Points rectangleHeight = 30;

			var rectangles = Layout.StackFromTop(
				Shapes.Rectangle(
						fill: Theme.Shadow,
						cornerRadius: Observable.Return(new CornerRadius(2)))
					.WithSize(new Size<Points>(rectangleWidth * 0.8, rectangleHeight))
					.DockLeft(),
				Spacer.Small,
				Shapes.Rectangle(
						stroke: Theme.SelectionStroke(Observable.Return(false), Observable.Return(true), Observable.Return(false)),
						fill: Theme.PanelBackground,
						cornerRadius: Observable.Return(new CornerRadius(2)))
					.WithSize(new Size<Points>(rectangleWidth, rectangleHeight))
					.DockLeft(),
				Spacer.Small,
				Shapes.Rectangle(
						fill: Theme.Shadow,
						cornerRadius: Observable.Return(new CornerRadius(2)))
					.WithSize(new Size<Points>(rectangleWidth * 0.6, rectangleHeight))
					.DockLeft());

			return Layout.StackFromTop(
					rectangles.CenterHorizontally()
						.WithOverlay(Arrow().WithPadding(new Thickness<Points>(rectangleWidth * 0.99, rectangleHeight * 1.3, 0, 0)).Center()),
					Spacer.Medium,
					Label.Create(
						Texts.Inspector_SelectSomething,
						color: Theme.DefaultText,
						font: Theme.DefaultFont))
				.Center()
				.WithBackground(Shapes.Rectangle(fill: Theme.PanelBackground))
				.MakeHittable()
				.Control;
		}

		static IObservable<Optional<string>> UneditableElementMessage(IProject project)
		{
			return project.Context.CurrentSelection.Is("Fuse.Triggers.Trigger")
				.CombineLatest(project.Context.CurrentSelection.Is("Fuse.Animations.Animator"),
					(isTrigger, isAnimator) =>
					{
						if (isTrigger || isAnimator)
							return Optional.Some(string.Format("Currently you can't edit {0}.\r\nYou'll have to do it manually.", isTrigger ? "Triggers" : "Animators"));
						return Optional.None();
					})
				.DistinctUntilChanged()
				.Replay(1)
				.RefCount();
		}

		static IControl UneditablePlaceholder(IObservable<Optional<string>> uneditableElementMessage)
		{
			return Layout.StackFromTop(
					Icons.CannotEditPlaceholder().WithPadding(bottom: new Points(20)),
					Label.Create(
						text: uneditableElementMessage.Select(x => x.OrDefault()).AsText(),
						font: Theme.DefaultFont,
						color: Theme.DisabledText))
				.Center();
		}

		static IControl Arrow()
		{
			var enabledIcon = Image.FromResource("Outracks.Fuse.Icons.selection_icon_on.png", typeof(Inspector).Assembly, overlayColor: Theme.PanelBackground);
			var disabledIcon = Image.FromResource("Outracks.Fuse.Icons.selection_icon_off.png", typeof(Inspector).Assembly, overlayColor: Theme.FieldFocusStroke.Brush);
			return disabledIcon.WithBackground(enabledIcon).WithSize(new Size<Points>(30, 30));
		}

	}


	static class Rows
	{
		public static IControl NameRow(this IEditorFactory editors, string name, IAttribute<string> property, bool deferEdit = false)
		{
			return Layout.Dock()
				.Left(editors.Label(name, property).WithWidth(CellLayout.FullCellWidth))
				.Left(Spacer.Small)
				.Fill(editors.Field(property, placeholderText: "Add name here", deferEdit: deferEdit));
		}
	}
}
