using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Outracks.CodeCompletion;
using Uno.Compiler.API.Domain;
using Uno.Compiler.API.Domain.IL;
using Uno.Compiler.API.Domain.IL.Members;
using Uno.Compiler.API.Domain.IL.Types;

namespace Outracks.UnoDevelop.UXNinja
{
	class StartTagNameSuggestion : ISuggestion
	{
		readonly DataTypes _dataTypes;
		readonly IElementContext _parentElement;
		readonly IElementContext _targetElement;
		readonly IDictionary<string, string> _namespaceDeclarations;

		public StartTagNameSuggestion(DataTypes dataTypes, IElementContext parentElement, IElementContext targetElement, IDictionary<string, string> namespaceDeclarations)
		{
			_dataTypes = dataTypes;
			_parentElement = parentElement;
			_targetElement = targetElement;
			_namespaceDeclarations = namespaceDeclarations;
		}

		public IEnumerable<SuggestItem> Suggest()
		{
			if (_parentElement == null)
			{
				var dataTypes = DataTypeHelper.GetDataTypesForElement(_targetElement, _dataTypes, _namespaceDeclarations);
				return ConvertNamescopesToSuggestions(dataTypes);
			}

			var allTypes = _dataTypes.AccessibleUxTypes;
			var concreteTypes = allTypes.Where(dataType => !dataType.Modifiers.HasFlag(Modifiers.Abstract))
				.ToArray();

			var parentDataType = _parentElement.ToDataType(_parentElement.Name == "App" ? allTypes : concreteTypes, _namespaceDeclarations);
			if (parentDataType == null) return Enumerable.Empty<SuggestItem>();

			var parentPublicProperties = Task.Run(() => PropertyHelper.GetAllComponentPrimaryOrContentProperties(parentDataType));

			var hasPrefix = HasTargetNamespacePrefix();
			if (hasPrefix)
				return HandleNamespacePrefix(concreteTypes, parentPublicProperties);

			var ns = DataTypeHelper.GetQualifiedNamespaceForElement(_targetElement);
			var types = HandleNonNamespacePrefix(DataTypeHelper.GetDataTypesForElement(_targetElement, _dataTypes, _namespaceDeclarations).ToArray(), parentPublicProperties);

			if (ns != null)
			{
				return types.Concat(ConvertNamescopesToSuggestions(GetNamespacesInNamespace(ns)));
			}
			else
			{
				return types
					.Concat(ConvertNamescopesToSuggestions(GetNamespacesInRoot()))
					.Concat(_namespaceDeclarations.Values.Where(nss => nss != "")
						.Select(nss => SuggestionHelper.Suggest(SuggestItemType.Namespace, null, nss + ":")));
			}
		}

		IEnumerable<SuggestItem> HandleNamespacePrefix(IEnumerable<DataType> dataTypes, Task<IEnumerable<Property>> parentProperties)
		{
			var ns = DataTypeHelper.ResolveNamespacesForNamespacePrefix(
				_targetElement.NamespacePrefix,
				_namespaceDeclarations);

			return ConvertNamescopesToSuggestions(
				dataTypes.Where(d =>
						IsDataTypeInNamespaces(d, ns)
						&& IsDataTypeValidAsChildInParent(d, parentProperties.Result)));
		}

		IEnumerable<SuggestItem> HandleNonNamespacePrefix(IEnumerable<DataType> dataTypes, Task<IEnumerable<Property>> parentProperties)
		{
			return ConvertNamescopesToSuggestions(
				dataTypes.Where(d => IsDataTypeValidAsChildInParent(d, parentProperties.Result)));
		}

		IEnumerable<Namescope> GetNamespacesInRoot()
		{
			return _dataTypes.RootNamespace.Namespaces;
		}

		IEnumerable<Namescope> GetNamespacesInNamespace(string ns)
		{
			var nss = ns.Split('.');

			var r = _dataTypes.RootNamespace;

			foreach (var p in nss)
			{
				if (r == null) break;
				r = r.Namespaces.FirstOrDefault(x => x.Name == p);
			}

			if (r != null)
				foreach (var c in r.Namespaces)
					yield return c;
		}

		IEnumerable<SuggestItem> ConvertNamescopesToSuggestions(IEnumerable<Namescope> dataTypes)
		{
			return dataTypes.Select(d =>
				{
					var itemType = SuggestItemType.Class;
					if (d is Namespace) itemType = SuggestItemType.Namespace;
					return SuggestionHelper.Suggest(itemType, d, TypeAliases.HasAlias(d.QualifiedName) ? d.FullName : d.Name);
				});
		}

		static bool IsDataTypeInNamespaces(DataType dataType, IEnumerable<string> nsNames)
		{
			var parent = dataType.Parent;
			return parent != null
				&& parent.NamescopeType == NamescopeType.Namespace
				&& nsNames.Contains(parent.FullName);
		}

		static bool IsDataTypeValidAsChildInParent(DataType dataType, IEnumerable<Property> parentPublicProperties)
		{
			return parentPublicProperties.Any(publicProperty => IsCompatibleAsDataInMember(dataType, publicProperty.ReturnType));
		}

		static bool IsCompatibleAsDataInMember(DataType data, DataType member)
		{
			if (data.IsCompatibleWith(member))
				return true;

			if (IsCompatibleAsDataInInterfaces(data, member.Interfaces))
				return true;

			if (PropertyHelper.IsMemberAListAndDataTypeCompatibleWithIt(data, member))
				return true;

			return false;
		}

		static bool IsCompatibleAsDataInInterfaces(DataType data, IEnumerable<InterfaceType> interfaceTypes)
		{
			if (interfaceTypes == null) return false;
			foreach (var interfaceType in interfaceTypes)
			{
				if (data.IsCompatibleWith(interfaceType))
					return true;

				if (PropertyHelper.IsMemberAListAndDataTypeCompatibleWithIt(data, interfaceType))
					return true;
			}

			return false;
		}

		bool HasTargetNamespacePrefix()
		{
			return !string.IsNullOrEmpty(_targetElement.NamespacePrefix);
		}
	}
}