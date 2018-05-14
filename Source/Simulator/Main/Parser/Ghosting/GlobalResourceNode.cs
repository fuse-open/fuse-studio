using System.Collections.Generic;
using System.Xml.Linq;

namespace Outracks.Simulator.Parser
{
	sealed class GlobalResourceNode : IMemberNode
	{
		public static IEnumerable<GlobalResourceNode> GetGlobalResourcesIn(XElement elm)
		{
			foreach (var child in elm.DescendantsInDocumentScope())
			{
				var globalAttr = child.TryGetUxAttribute("Global");
				if (globalAttr.HasValue)
					yield return new GlobalResourceNode(globalAttr.Value, child.Name.LocalName);
			}
		}

		public string Name { get; private set; }
		public string TypeName { get; private set; }

		public GlobalResourceNode(string name, string typeName)
		{
			Name = name;
			TypeName = typeName;
		}
	}
}