using System.Collections.Generic;
using System.Linq;
using System;
using Uno.Compiler.API;
using Uno.Compiler.API.Domain;
using Uno.Compiler.API.Domain.IL;

namespace Outracks.UnoDevelop.UXNinja
{
	public static class DataTypeHelper
	{
		public static bool IsOfType(this DataType dt, Type type)
		{
			// Early out test
			if (dt.Name != type.Name)
				return false;

			return dt.FullName == type.FullName;
		}

		public static DataType ResolveDataTypeFromName(string name, IEnumerable<DataType> dataTypes, IEnumerable<KeyValuePair<string, string>> namespaces)
		{
			var namespacePrefix = ExtractNamespacePrefix(name);
			var nameExclPrefix = ExtractNameExclPrefix(name);

			var elmNamespaces = ResolveNamespacesForNamespacePrefix(namespacePrefix, namespaces);
			return dataTypes.FirstOrDefault(d => IsNameThisDataType(nameExclPrefix, elmNamespaces, d));
		}

		public static string ExtractNamespacePrefix(string name)
		{
			var idxNsDelimiter = name.IndexOf(":", StringComparison.Ordinal);
			var namespacePrefix = idxNsDelimiter >= 0 ? name.Substring(0, idxNsDelimiter) : "";

			return namespacePrefix;
		}

		public static string ExtractNameExclPrefix(string name)
		{
			var idxNsDelimiter = name.IndexOf(":", StringComparison.Ordinal);
			var nameExclPrefix = idxNsDelimiter + 1 < name.Length
				&& idxNsDelimiter >= 0 ? name.Substring(idxNsDelimiter + 1) : name;

			return nameExclPrefix;
		}

		public static IEnumerable<string> ResolveNamespacesForNamespacePrefix(string namespacePrefix, IEnumerable<KeyValuePair<string, string>> namespaces)
		{
			var elmNamespaces = new List<string>();
			
			foreach (var namesp in namespaces)
			{
				if(namesp.Value == namespacePrefix)
					elmNamespaces.Add(namesp.Key);
			}

			return elmNamespaces;
		}

		public static bool IsNameThisDataType(string name, IEnumerable<string> elmNamespaces, DataType dataType)
		{
			var dataFullName = dataType.QualifiedName;

			return (dataType.Parent != null && dataType.Parent.IsRoot && dataFullName == name)
				|| elmNamespaces.Select(elmNamespace => elmNamespace + "." + name).Any(fullName => fullName == dataFullName);
		}

		public static string GetQualifiedNamespaceForElement(IElementContext element)
		{
			return element.Name.Contains('.')
				? element.Name.Substring(0, element.Name.LastIndexOf('.'))
				: null;
		}

		static string[] GetNamespacesForElement(IElementContext element, IDictionary<string, string> namespaceDeclarations)
		{
			var ns = GetQualifiedNamespaceForElement(element);
			if (ns != null) return new string[] { ns };
			return namespaceDeclarations.Keys.ToArray();
		}

		public static IEnumerable<DataType> GetDataTypesForElement(IElementContext element, DataTypes dataTypes, IDictionary<string, string> namespaceDeclarations)
		{
			var ns = GetNamespacesForElement(element, namespaceDeclarations);
			return GetDataTypesInNamespaces(dataTypes, ns);
		}

		static IEnumerable<DataType> GetDataTypesInNamespaces(DataTypes dataTypes, params string[] ns)
		{
			return dataTypes.AccessibleUxTypes.Where(d => IsParentNamespaceInContextNamespaces(d, ns));
		}

		static bool IsParentNamespaceInContextNamespaces(DataType dataType, ICollection<string> namespaceDeclarations)
		{
			if (dataType.Parent == null || dataType.Parent.NamescopeType != NamescopeType.Namespace)
				return false;

			return dataType.Parent.IsRoot
				|| namespaceDeclarations.Contains(dataType.Parent.FullName)
				|| TypeAliases.HasAlias(dataType.QualifiedName);
		}
	}
}