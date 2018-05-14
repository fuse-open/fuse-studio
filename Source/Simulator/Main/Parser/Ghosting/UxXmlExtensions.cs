using System.Collections.Generic;
using System.Xml.Linq;
using Uno.UX.Markup;

namespace Outracks.Simulator.Parser
{
	static class UxXmlExtensions
	{
		public static IEnumerable<XElement> DescendantsInDocumentScope(this XElement element, XElement parentScope = null)
		{
			parentScope = parentScope ?? element;

			foreach (var child in element.Elements())
			{
				yield return child;
				if (!child.TryGetUxAttribute("Class").HasValue)
					foreach (var nonDirectChild in DescendantsInDocumentScope(child, parentScope))
						yield return nonDirectChild;
			}
		}


		public static Optional<string> TryGetUxAttribute(this XElement elm, string name)
		{
			foreach (var t in elm.Attributes())
			{
				if (t.Name.NamespaceName == Configuration.UXNamespace && t.Name.LocalName == name)
					return t.Value;
			}

			return Optional.None();
		}
	}
}