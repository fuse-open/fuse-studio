using System.Collections.Generic;
using System.Linq;
using Outracks.CodeCompletion;
using Uno.Compiler.API;
using Uno.Compiler.API.Domain.IL;
using Uno.UX.Markup.Common;

namespace Outracks.UnoDevelop.UXNinja
{
	class StandardAttributesSuggestion : ISuggestion
	{
		readonly DataTypes _dataTypes;
		readonly IElementContext _targetElement;
		readonly IElementContext _parentElement;
		readonly string _attributeName;
		readonly ICodeReader _codeReader;
		readonly IDictionary<string, string> _namespaces;

		static readonly string[] _standardAttributeNames = { "xmlns" };
		public static IEnumerable<string> StandardAttributeNames
		{
			get
			{
				foreach (var name in _standardAttributeNames)
					yield return name;

				foreach (var name in Attributes.Values)
					yield return "ux:" + name;
			}
		}

		public StandardAttributesSuggestion(DataTypes dataTypes, IElementContext targetElement, IElementContext parentElement, string attributeName, ICodeReader codeReader, IDictionary<string, string> namespaces)
		{
			_dataTypes = dataTypes;
			_targetElement = targetElement;
			_parentElement = parentElement;
			_attributeName = attributeName;
			_codeReader = codeReader;
			_namespaces = namespaces;
		}

		public IEnumerable<SuggestItem> Suggest()
		{
			switch (_attributeName)
			{
				case "ux:Binding":
					return SuggestBindingValues();

				// TODO: Implement ux code reader 
				/*case "ux:Ref":
					return SuggestReferenceValue();*/
			}

			return null;
		}

		IEnumerable<SuggestItem> SuggestBindingValues()
		{
			var dataType = _parentElement.ToDataType(_dataTypes.AccessibleUxTypes, _namespaces);
			if (dataType == null) return null;

			var properties = PropertyHelper.GetAllWriteableProperties(dataType);
			return properties.Select(p => SuggestionHelper.Suggest(SuggestItemType.Property, p, p.Name));
		}

		IEnumerable<SuggestItem> SuggestReferenceValue()
		{
			var nameAttributes = FindUXNameAttributes(_codeReader);
			var namesToDataTypes = ResolveNamesAndTypes(_dataTypes, nameAttributes, _namespaces);

			var elementDataType = _targetElement.ToDataType(_dataTypes.AccessibleUxTypes, _namespaces);
			if (elementDataType == null) return null;

			return namesToDataTypes.Where(d => d.Value == elementDataType).Select(n => SuggestionHelper.Suggest(SuggestItemType.Variable, n.Value, n.Key));
		}

		public static IEnumerable<KeyValuePair<string, DataType>> ResolveNamesAndTypes(DataTypes dataTypes, 
			IEnumerable<KeyValuePair<string, string>> nameAttributes, IDictionary<string, string> namespaces)
		{
			var nameToDataType = new Dictionary<string, DataType>();
			foreach (var nameAttribute in nameAttributes)
			{
				var dataType = DataTypeHelper.ResolveDataTypeFromName(nameAttribute.Key, dataTypes.AccessibleUxTypes, namespaces);
				if (dataType == null) continue;
				
				nameToDataType.Add(nameAttribute.Value, dataType);
			}

			return nameToDataType;
		}

		public static IEnumerable<KeyValuePair<string, string>> FindUXNameAttributes(ICodeReader codeReader)
		{
			var offset = codeReader.Offset;
			codeReader.Offset = 0;

			var nameAttributes = new Dictionary<string, string>();
			var lastTagName = "";
			var lastAttributeName = "";

			while (codeReader.Offset < codeReader.Length)
			{
				switch (codeReader.PeekToken())
				{
					case TokenType.StartTagName:
						lastTagName = ParseTokenText(codeReader);
						break;
					case TokenType.StartTagAttributeName:
						lastAttributeName = ParseTokenText(codeReader);
						break;
					case TokenType.StartTagAttributeValueText:
						string attributeValue = ParseTokenText(codeReader);
						if (lastAttributeName == "ux:Name")
							nameAttributes.Add(lastTagName, attributeValue);

						break;                    
					default:
						codeReader.ReadToken();
						break;
				}
			}
			codeReader.Offset = offset;

			return nameAttributes;
		}

		public static string ParseTokenText(ICodeReader reader)
		{
			var offsetStart = reader.Offset;
			reader.ReadToken();
			var offsetEnd = reader.Offset;
			return reader.PeekTextReverse(offsetEnd - offsetStart);
		}
	}
}