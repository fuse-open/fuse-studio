using System.Collections.Generic;
using System.Linq;
using Uno.Compiler.API.Domain.IL;

namespace Outracks.UnoDevelop.UXNinja
{
	public static class ElementContextExtensions
	{
		public static DataType ToDataType(this IElementContext element, IEnumerable<DataType> dataTypes, IEnumerable<KeyValuePair<string, string>> namespaces)
		{
			if (element.Name.Contains('.'))
			{
				return dataTypes.FirstOrDefault(d => d.QualifiedName == element.Name);
			}

			var elmNamespaces = DataTypeHelper.ResolveNamespacesForNamespacePrefix(element.NamespacePrefix, namespaces);

			return dataTypes.FirstOrDefault(d => DataTypeHelper.IsNameThisDataType(element.Name, elmNamespaces, d));
		}

		public static bool HasAttributeWithNamespace(this IElementContext element, string ns, string attributeName)
		{
			return element.Attributes.Any(attr => attr.NamespacePrefix == ns && attr.Name == attributeName);
		}

		public static string GetAttributeValue(this IElementContext element, string ns, string attributeName)
		{
			var attrib = element.Attributes.FirstOrDefault(attr => attr.NamespacePrefix == ns && attr.Name == attributeName);
			return attrib == null ? null : attrib.Value;
		}
	}
}