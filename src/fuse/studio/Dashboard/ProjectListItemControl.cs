using System;
using System.Reactive.Linq;
using Outracks.Fusion;

namespace Outracks.Fuse.Dashboard
{
	class ProjectListItemControl
	{
		public static IControl Create(IObservable<ProjectListItem> item, SelectionState state)
		{
			return Layout
				.StackFromLeft(
					item.Select(i => i.Thumbnail).Switch()
					.WithSize(Size.Create<Points>(80, 145))
					.WithOverlay(
						Shapes.Rectangle(
							stroke: state.IsSelected.Select(c => c ? Stroke.Create(2, Theme.Active) : Stroke.Create(0, Color.Transparent)).Switch(),
							cornerRadius: Observable.Return(new CornerRadius(2))))
					.CenterHorizontally(),
					Layout.StackFromTop(
						Label.Create(
							item.Select(i => i.Title).AsText(),
							textAlignment: TextAlignment.Left,
							lineBreakMode: LineBreakMode.Wrap,
							color: state.IsSelected.Select(c => c ? Theme.Active : Theme.DefaultText).Switch(),
							font: Font.SystemDefault(Observable.Return(19.0))),
						Spacer.Medium,
						Label.Create(
							text: item.Select(i => i.DescriptionTitle)
								.AsText(),
							color: Theme.DescriptorText,
							font: Theme.DescriptorFont),
						Label.Create(
							item.Select(i => i.Description.Wrap(28) + ".")
								.AsText(),
							color: Theme.DefaultText,
							lineBreakMode: LineBreakMode.Wrap,	// FIXME: Doesn't work?
							font: Theme.DescriptorFont),
						Spacer.Small,
						Label.Create(
							text: item.Select(i => i.LocationTitle)
								.AsText(),
							color: Theme.DescriptorText,
							font: Theme.DescriptorFont),
						Label.Create(
							text:item.Select(i => i.Location)
								.AsText(),
							lineBreakMode: LineBreakMode.TruncateTail,
							color: Theme.DefaultText,
							font: Theme.DescriptorFont))
					.CenterVertically()
					.WithWidth(200)
					.WithPadding( left: new Points(16)))
				.WithPadding(new Thickness<Points>(0, 24));
		}
	}
}
