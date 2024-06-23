using System;

namespace Outracks.Fuse.Inspector.Sections
{
	static class ElementDockPanelExtensions
	{
		public static IObservable<bool> IsInDockPanelContext(this IElement element)
		{
			return element.IsChildOf("Fuse.Controls.DockPanel")
				.Or(element.IsSiblingOf("Fuse.Layouts.DockLayout"));
		}
	}
}