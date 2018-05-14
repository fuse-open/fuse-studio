using System.Collections.Generic;
using System.Xml.Linq;

namespace Outracks.Simulator.Parser
{
	sealed class PropertyNode : IMemberNode
	{
		public static IEnumerable<PropertyNode> GetPropertiesIn(XElement elm)
		{
			foreach (var child in elm.DescendantsInDocumentScope())
			{
				var globalAttr = child.TryGetUxAttribute("Property");
				if (globalAttr.HasValue)
					yield return new PropertyNode(globalAttr.Value, child.Name.LocalName);
			}
		}

		public string Name { get; private set; }
		public string TypeName { get; private set; }

		public PropertyNode(string name, string typeName)
		{
			Name = name;
			TypeName = typeName;
		}
	}
}