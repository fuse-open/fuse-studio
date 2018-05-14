using System;
using System.Collections.Generic;
using System.IO;
using System.Xml.Linq;

namespace Outracks.Simulator.Parser
{
	class OuterClassNode
	{
		public static IEnumerable<OuterClassNode> GetOuterClassNodes(ParsedDocument doc)
		{
			var root = doc.RootElement;
			{
				var classAttrValue = root.TryGetUxAttribute("Class");
				var classNameAttrValue = root.TryGetUxAttribute("ClassName");
				var className = classAttrValue.Or(classNameAttrValue);
				var classNameForApp = Path.GetFileNameWithoutExtension(doc.Path);

				if (className.HasValue || IsApp(root))
					yield return new OuterClassNode(
						className.Or(classNameForApp),
						root.Name.LocalName,
						doc.Path,
						GlobalResourceNode.GetGlobalResourcesIn(root),
						PropertyNode.GetPropertiesIn(root));
			}

			foreach (var child in doc.RootElement.Descendants())
			{
				var classAttrValue = child.TryGetUxAttribute("Class");
				if (classAttrValue.HasValue)
					yield return new OuterClassNode(
						classAttrValue.Value, 
						child.Name.LocalName, 
						doc.Path, 
						GlobalResourceNode.GetGlobalResourcesIn(child),
						PropertyNode.GetPropertiesIn(child));
			}
		}

		static bool IsApp(XElement element)
		{
			return element.Name.LocalName == "App" || element.Name.LocalName == "Fuse.App";
		}

		public readonly string DeclaringFile;
		public readonly string GeneratedTypeName;
		public readonly string BaseTypeName;
		public readonly ImmutableList<GlobalResourceNode> GlobalResources;
		public readonly ImmutableList<PropertyNode> Properties;

		public OuterClassNode(
			string generatedTypeName, 
			string baseTypeName,
			string declaringFile, 
			IEnumerable<GlobalResourceNode> globalResources,
			IEnumerable<PropertyNode> properties)
		{
			GeneratedTypeName = generatedTypeName;
			BaseTypeName = baseTypeName;
			DeclaringFile = declaringFile;
			Properties = properties.ToImmutableList();
			GlobalResources = globalResources.ToImmutableList();
		}
	}
}