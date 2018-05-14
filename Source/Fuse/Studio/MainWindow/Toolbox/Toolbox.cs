using System;
using System.Reactive.Linq;
using Outracks.Fuse.Inspector.Editors;
using Uno.Collections;

namespace Outracks.Fuse.Toolbox
{
	using Fusion;

	public static class Toolbox
	{
		
		public static IControl Create(IProject project, ElementContext elementContext)
		{
			var m = Spacer.Medium.DesiredSize.Width;
			var s = Spacer.Small.DesiredSize.Width;

			var hasClasses = project.Classes.Select(x => x.Any());
			var searchString = Property.Create("");
			
			return Layout.Dock()
				.Top(Layout.Dock()
					.Top(Separator.Medium)
					.Bottom(Separator.Weak)
					.Bottom(Separator.Medium)
					.Left(Spacer.Medium)
					.Left(Icons.ClassesSmall())
					.Left(Spacer.Small)
					.Left(Theme.Header("Your classes"))
					.Fill()
					.WithHeight(26))
				.Top(ThemedTextBox
					.Create(searchString)
					.WithPlaceholderText(searchString.Select(str => str != ""), "Search your classes")
					.WithMediumPadding()
					.ShowWhen(hasClasses))
				.Top(	
					Layout.StackFromLeft(
						Icons.DragIconSmall(),
						Spacer.Small,
						Label.Create(
							"Drag classes into the hierarchy \nto insert instances of them",
							font: Theme.DefaultFont,
							textAlignment: TextAlignment.Left,
							color: Theme.DescriptorText),
						Spacer.Medium)
					.ShowWhen(hasClasses)
					.CenterHorizontally())
				.Top(
					Layout.StackFromLeft(
							Icons.ExtractClass(hasClasses),
							Spacer.Small,
							Label.Create(
								"Create a class from elements in the \nhierarchy and it will appear here",
								font: Theme.DefaultFont,
								textAlignment: TextAlignment.Left,
								color: Theme.DescriptorText),
							Spacer.Medium)
					.WithMediumPadding()
					.HideWhen(hasClasses)
					.CenterHorizontally())
				.Fill(
				project.Classes
					.CachePerElement(e => CreateClassItem(e, project.Context, elementContext, searchString))
					.StackFromTop()
					.WithPadding(left: m, top: s, right: m, bottom: s)
					.MakeScrollable(darkTheme: Theme.IsDark))
				.MakeResizable(RectangleEdge.Top, "MyClassesHeight");
		}

		static IControl CreateClassItem(
			IElement classElement,
			IContext context,
			ElementContext elementContext,
			IObservable<string> searchString)
		{
			var className = classElement["ux:Class"].Or("");
			return Layout
				.StackFromLeft(
						classElement.MediumIcon(Theme.Purple, Theme.IconSecondary).CenterVertically(),
						Spacer.Small,
						Label.Create(className.AsText(), font:Theme.DefaultFont, color: Theme.DefaultText).CenterVertically())
					.WithPadding(new Thickness<Points>(5))
				.WithBackground(Shapes
					.Rectangle(
						fill: Color.FromRgb(0xaaaaaa).WithAlpha(a: 0.2f), 
						cornerRadius: Observable.Return(new CornerRadius(2)))
					.ShowWhen(context.IsSelected(classElement)))
				.DockLeft()
				.OnMouse(
					entered: Command.Enabled(() => context.Preview(classElement)),
					exited: Command.Enabled(() => context.Preview(Element.Empty)),
					pressed: Command.Enabled(() => context.Select(classElement)),
					dragged: className.Select(er => SourceFragment.FromString("<" + er + "/>")),
					doubleClicked: Command.Enabled(() =>
						context.SetScope(root: classElement, selection: classElement)))
				.SetContextMenu(elementContext.CreateMenu(classElement))
				.WithHeight(26)
				.ShowWhen(
					Observable.CombineLatest(
						searchString,
						className.AsText(),
						(search, name) =>
						{
							if (search == "") return true;
							return name.IndexOf(search, StringComparison.OrdinalIgnoreCase) >= 0; // case insensitive comparison
						}));
		}
	}
}