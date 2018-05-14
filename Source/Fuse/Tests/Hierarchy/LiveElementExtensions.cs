using System.Collections.Generic;
using System.Linq;
using Outracks.Fuse.Live;

namespace Outracks.Fuse.Tests.Hierarchy
{
	internal static class LiveElementExtensions
	{
		public static IEnumerable<ILiveElement> DescendantsAndSelf(this ILiveElement element)
		{
			yield return element;
			foreach (var descendant in element.Children.Value.SelectMany(DescendantsAndSelf))
				yield return descendant;
		}

		public static void UpdateFrom(this LiveElement element, string xml)
		{
			element.UpdateFrom(SourceFragment.FromString(xml).ToXml());
		}
	}
}