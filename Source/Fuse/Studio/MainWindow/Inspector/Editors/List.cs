using System;
using System.Linq;
using System.Reactive.Linq;

namespace Outracks.Fuse.Inspector.Editors
{
	using Fusion;
	
	public static class ListEditor
	{
		public static IControl Create(IElement parent, Text name, SourceFragment fragment, Func<IElement, IControl> content)
		{
			var type = fragment.ToXml().Name.LocalName;
			var children = parent.Children
				.Where(e => e.Name.Is(type))
				.Replay(1).RefCount();

			var hasContent = children.Select(c => c.Any());
			
			var selectedChild = children.Select(c => c.LastOrNone().Or(Element.Empty)).Switch();

			var stackedContent = children
				.PoolPerElement(e => content(e.Switch()))
				.StackFromTop(separator: () => Spacer.Medium);

			var textColor =  parent.IsReadOnly().Select(ro => ro 
				? Theme.DisabledText 
				: Theme.DefaultText).Switch();

			return Layout.StackFromTop(
				Separator.Weak,
				Layout.Dock()
					.Left(Label.Create(name, Theme.DefaultFont, color: textColor)
						.CenterVertically())
					.Right(ListButtons.AddButton(() => parent.Paste(fragment), isEnabled:  parent.IsReadOnly().IsFalse())
						.CenterVertically())
					.Right(Spacer.Small)
					.Right(ListButtons.RemoveButton(() => selectedChild.Cut(), isEnabled: selectedChild.IsReadOnly().IsFalse())
						.CenterVertically())
					.Fill()
					.WithHeight(30)
					.WithInspectorPadding(),
				Separator.Weak.ShowWhen(hasContent),
				Separator.Weak.ShowWhen(hasContent),
				Layout.StackFromTop(
						Spacer.Medium, 
						stackedContent, 
						Spacer.Medium,
						Separator.Weak.ShowWhen(hasContent))
					.MakeCollapsable(RectangleEdge.Bottom, hasContent));
		}

		// TODO: make something proper
		static IObservable<bool> IsReadOnly(this IElement element)
		{
			return element.SimulatorId.Select(id => id.Equals(Simulator.ObjectIdentifier.None));
		}
	}
}