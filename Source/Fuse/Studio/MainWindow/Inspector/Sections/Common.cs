using System;
using Outracks.Fuse.Designer;

namespace Outracks.Fuse.Inspector.Sections
{
	using Fusion;

	class CommonSection
	{
		public static IControl Create(IElement element, IProject project, IEditorFactory editors, IPopover popover)
		{
			return Layout.StackFromTop(
				popover.CreatePopover(RectangleEdge.Bottom, 
					dialog => Button.Create(
						dialog.IsVisible.Toggle(), 
						state => Layout.StackFromTop(
							Spacer.Medium,
							Layout.StackFromLeft(
									element.MediumIcon(Theme.IconPrimary, Theme.IconSecondary)
										.CenterVertically(),
									Spacer.Small, 
									Label.Create(element.Name.AsText(), color: Theme.DefaultText, font: Theme.HeaderFont)
										.CenterVertically(),
									Spacer.Small,
									Layout.StackFromTop(
											Arrow.WithoutShaft(RectangleEdge.Top, SymbolSize.Small),
											Spacer.Smaller,
											Arrow.WithoutShaft(RectangleEdge.Bottom, SymbolSize.Small))
										.CenterVertically())
								.CenterHorizontally(),
							Spacer.Medium)),
					dialog =>
					{
						var result = Layout.Dock()
							.Bottom(Button.Create(Command.Enabled(() => dialog.IsVisible.OnNext(false)), bs =>
								Layout.Dock()
									.Left(Icons.Confirm(Theme.Active).CenterVertically())
									.Left(Spacer.Small)
									.Fill(Theme.Header("Done"))
									.Center()
									.WithHeight(30)))
							.Bottom(Separator.Medium)
							.Top(Spacer.Medium)
							.Top(Label.Create(text: "Replace element", textAlignment: TextAlignment.Center, font: Theme.DefaultFont, color: Theme.DefaultText))
							.Top(Spacer.Small)
							.Top(Label.Create(text: "Enter a new element type to replace \n the current element type", textAlignment: TextAlignment.Center, font: Theme.DescriptorFont, color: Theme.DescriptorText))
							.Top(Spacer.Medium)
							.Left(Spacer.Medium)
							.Right(Spacer.Medium)
							.Bottom(Spacer.Medium)
							.Fill(
								TextBox.Create(
									text: element.Name.Deferred(),
									foregroundColor: Theme.DefaultText)
								.WithPadding(new Thickness<Points>(1))
								.WithBackground(Theme.FieldBackground)
								.WithOverlay(Shapes.Rectangle(stroke: Theme.FieldStroke)))
							.WithWidth(279);

						var elementChanged = element.SimulatorId;
						elementChanged.ConnectWhile(result.IsRooted).Subscribe(id => dialog.IsVisible.OnNext(false));

						return result;
					}),
				
				Separator.Weak, Spacer.Medium,

				editors.NameRow("Instance Name", element.UxName(), deferEdit: true)
					.WithInspectorPadding(),
					
				Spacer.Medium, Separator.Weak,

				ContextualSections(element, project, editors));
		}

		public static IControl ContextualSections(IElement element, IProject project, IEditorFactory editors)
		{
			return Layout.StackFromTop(
				CircleSection.Create(element, editors),
				EachSection.Create(element, editors),
				GridSection.Create(element, editors),
				ImageSection.Create(project, element, editors),
				ScrollViewSection.Create(element, editors),
				StackPanelSection.Create(element, editors),
				TextSection.Create(project, element, editors),
				TextInputSection.Create(project, element, editors),
				WrapPanelSection.Create(element, editors));
		}
	}
}
