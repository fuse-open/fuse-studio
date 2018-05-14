using System.Collections.Generic;
using Uno;

namespace Outracks.UnoDevelop.UXNinja
{
	public interface IElementContext
	{
		string NamespacePrefix { get; }

		string Name { get; }

		IEnumerable<IAttributeContext> Attributes { get; }

		IEnumerable<IElementContext> Children { get; }
			
		Source StartTagSource { get; }

		Source EndTagSource { get; }

		IElementContext Parent { get; }

		T GetBackingStore<T>() where T : class;
	}

	public static class ElementContextTraversing
	{
		public static IEnumerable<IElementContext> TraverseSelfAndAllChildren(this IElementContext element)
		{
			yield return element;

			foreach (var child in element.Children)
			{
				foreach (var i in TraverseSelfAndAllChildren(child))
					yield return i;
			}
		}

		public static IEnumerable<IElementContext> TraverseAllChildren(this IElementContext element)
		{
			foreach (var child in element.Children)
			{
				foreach (var i in TraverseSelfAndAllChildren(child))
					yield return i;
			}
		}

		public static IEnumerable<IElementContext> TraverseAncestors(this IElementContext element)
		{
			if (element.Parent == null)
				yield break;

			yield return element.Parent;
			foreach (var parent in TraverseAncestors(element.Parent))
			{
				yield return parent;
			}
		}
	}
}
