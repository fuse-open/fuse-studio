using System.Collections.Generic;
using System.Linq;
using Outracks.CodeCompletion;
using Uno.Compiler.API;
using Uno.Compiler.API.Domain;
using Uno.Compiler.API.Domain.IL;
using Uno.Compiler.API.Domain.IL.Members;
using Uno.Compiler.API.Domain.IL.Types;

namespace Outracks.UnoDevelop.UXNinja
{
	class StartTagAttributeValueSuggestion : ISuggestion
	{
		readonly DataTypes _dataTypes;
		readonly IElementContext _targetElement;
		readonly IElementContext _parentElement;
		readonly IAttributeContext _lastAttribute;
		readonly ICodeReader _codeReader;
		readonly IDictionary<string, string> _namespaceDeclarations;        

		public StartTagAttributeValueSuggestion(
				DataTypes dataTypes,
				IElementContext targetElement, 
				IElementContext parentElement, 
				IAttributeContext lastAttribute, 
				ICodeReader codeReader, 
				IDictionary<string, string> namespaceDeclarations)
		{
			_dataTypes = dataTypes;
			_targetElement = targetElement;
			_parentElement = parentElement;
			_lastAttribute = lastAttribute;
			_codeReader = codeReader;
			_namespaceDeclarations = namespaceDeclarations;
		}

		public IEnumerable<SuggestItem> Suggest()
		{
			if (_targetElement == null || _lastAttribute == null) return Enumerable.Empty<SuggestItem>();

			if (_lastAttribute.NamespacePrefix == "ux" && _parentElement != null)
				return new StandardAttributesSuggestion(_dataTypes, _targetElement, _parentElement, 
					_lastAttribute.FullName, _codeReader, _namespaceDeclarations).Suggest();

			var dataType = _targetElement.ToDataType(_dataTypes.AccessibleUxTypes, _namespaceDeclarations);
			if (dataType == null) return Enumerable.Empty<SuggestItem>();

			var properties = PropertyHelper.GetAllWriteableProperties(dataType);
			var lastAttributeProperty = properties.FirstOrDefault(p => _lastAttribute.Name == p.Name);
			if (lastAttributeProperty == null) return Enumerable.Empty<SuggestItem>();

			return SuggestValuesForProperty(lastAttributeProperty);
		}

		IEnumerable<SuggestItem> SuggestValuesForProperty(Property lastAttributeProperty)
		{
			var valueDataType = lastAttributeProperty.ReturnType;
			if (valueDataType.BuiltinType != 0)
				return SuggestIntristic(valueDataType);

			if (valueDataType.TypeType == TypeType.Enum)
				return SuggestEnumLiterals(valueDataType as EnumType);

			// TODO: Implement ux code reader 
			/*if (valueDataType.Master.FullName == "Uno.UX.Property<>")
				return SuggestDesignerProperty(valueDataType);*/

			return null;
		}

		IEnumerable<SuggestItem> SuggestIntristic(DataType dataType)
		{
			switch (dataType.BuiltinType)
			{
				case BuiltinType.Bool:
					return SuggestBool(dataType);
			}

			return null;
		}

		IEnumerable<SuggestItem> SuggestEnumLiterals(EnumType enumType)
		{
			return enumType.Literals.Select(l => SuggestionHelper.Suggest(SuggestItemType.Constant, l, l.Name));
		}

		IEnumerable<SuggestItem> SuggestDesignerProperty(DataType valueType)
		{
			_codeReader.ReadTokenReverse();
			var nameAttributes = StandardAttributesSuggestion.FindUXNameAttributes(_codeReader);
			var dataTypes = StandardAttributesSuggestion.ResolveNamesAndTypes(_dataTypes,
																  nameAttributes,
																  _namespaceDeclarations);

			var attributeValue = StandardAttributesSuggestion.ParseTokenText(_codeReader);
			if (!attributeValue.Contains("."))
			{
				return dataTypes.Select(n => SuggestionHelper.Suggest(SuggestItemType.Variable, n.Value, n.Key));
			}

			var valuePieces = attributeValue.Split('.');
			var dataType = dataTypes.FirstOrDefault(d => d.Key == valuePieces[0]).Value;
			if (dataType == null) return null;

			var properties = PropertyHelper.GetAllWriteableProperties(dataType);
			var propertiesOfRightType = properties.Where(p => p.ReturnType == valueType.GenericArguments[0]);

			return propertiesOfRightType.Select(p => SuggestionHelper.Suggest(SuggestItemType.Property, p, valuePieces[0] + "." + p.Name));
		}
		
		IEnumerable<SuggestItem> SuggestBool(DataType dataType)
		{
			return new List<SuggestItem>()
			{
				SuggestionHelper.Suggest(SuggestItemType.Keyword, dataType, "true"),
				SuggestionHelper.Suggest(SuggestItemType.Keyword, dataType, "false")          
			};
		}
	}
}
