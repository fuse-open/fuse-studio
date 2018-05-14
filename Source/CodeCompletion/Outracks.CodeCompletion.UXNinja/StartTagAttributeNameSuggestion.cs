using System.Collections.Generic;
using System.Linq;
using Outracks.CodeCompletion;
using Uno.Compiler.API;
using System.Threading.Tasks;
using Uno.Compiler.API.Domain.IL;
using Uno.Compiler.API.Domain.IL.Expressions;
using Uno.Compiler.API.Domain.IL.Members;
using Uno.UX;

namespace Outracks.UnoDevelop.UXNinja
{
	class StartTagAttributeNameSuggestion : ISuggestion
	{
		readonly DataTypes _dataTypes;
		readonly IElementContext _elementContext;
		readonly IDictionary<string, string> _namespaceDeclarations;

		public StartTagAttributeNameSuggestion(DataTypes dataTypes, IElementContext elementContext, IDictionary<string, string> namespaceDeclarations)
		{
			_dataTypes = dataTypes;
			_elementContext = elementContext;
			_namespaceDeclarations = namespaceDeclarations;
		}

		public IEnumerable<SuggestItem> Suggest()
		{
			if (_elementContext == null) return Enumerable.Empty<SuggestItem>();

			var suggestItems = PossibleStandardAttribs();

			var dataType = _elementContext.ToDataType(_dataTypes.AccessibleUxTypes, _namespaceDeclarations);
			if(dataType == null) 
				return suggestItems;

			var possibleArguments = Task.Run(() => PossibleArguments(dataType));
			var possibleProperties = Task.Run(() => PossibleProperties (dataType));
			var possibleEvents = Task.Run(() => PossibleEvents (dataType)); 

			return suggestItems
				.Concat(possibleArguments.Result)
				.Concat(possibleProperties.Result)
				.Concat(possibleEvents.Result);
		}

		IEnumerable<SuggestItem> PossibleStandardAttribs()
		{
			var standardSuggestions = StandardAttributesSuggestion.StandardAttributeNames.Where(IsAttributeNotDefinedAlready);
			var suggestItems = standardSuggestions.Select(
				s =>
					SuggestionHelper.Suggest(
						SuggestItemType.Property,
						null,
						s,
						null,
						null,
						AutoCompletePropertyOnCommit));

			return suggestItems;
		}

		IEnumerable<SuggestItem> PossibleEvents(DataType dataType)
		{
			var validEvents = MemberHelper.GetAllPublicEvents(dataType)
				.Where(e => IsAttributeNotDefinedAlready(e.Name) && IsMemberNotHidden(e));

			var suggestItems = validEvents
				.Select(e => SuggestionHelper.Suggest(SuggestItemType.Event, e, e.Name, null, null, AutoCompletePropertyOnCommit));
			
			return suggestItems;
		}

		IEnumerable<SuggestItem> PossibleArguments(DataType dataType)
		{
			var args = PropertyHelper.GetAllConstructorArguments(dataType);
			var uxParameter = args.Select(p => p.Attributes.FirstOrDefault(a => a.ReturnType.IsOfType(typeof(UXParameterAttribute))));

			var uxParameterValue = uxParameter.Where(p => p != null)
									.Select(p => p.Arguments[0].ActualValue as Constant)
									.Where(p => p != null)
									.Select(p => (string)p.Value);

			var validArgs = uxParameterValue.Where(p => IsAttributeNotDefinedAlready(p));

			var suggestItems = validArgs
				.Select(uxParam => SuggestionHelper.Suggest(SuggestItemType.Property, uxParameter, uxParam, null, null, AutoCompletePropertyOnCommit));

			return suggestItems;
		}

		bool IsParameterValid(Parameter param)
		{
			return IsAttributeNotDefinedAlready(param.Name) && param.Attributes.OfType<UXParameterAttribute>().Any();
		}

		IEnumerable<SuggestItem> PossibleProperties(DataType dataType)
		{
			var properties = PropertyHelper.GetAllWriteableProperties(dataType);
			var validAttributes = properties.Where(p => IsAttributeNotDefinedAlready(p.Name) && IsMemberNotHidden(p));

			var suggestItems = validAttributes
				.Select(p => SuggestionHelper.Suggest(SuggestItemType.Property, p, p.Name, null, null, AutoCompletePropertyOnCommit));
			 
			return suggestItems;
		}

		bool IsAttributeNotDefinedAlready(string name)
		{
			return _elementContext.Attributes == null || _elementContext.Attributes.All(attribute => attribute.FullName != name);
		}

		bool IsMemberNotHidden(Member member)
		{
			return true;
				// The Fuse.Designer.HideAttribute si only intended for tooling, not UX accessibility
				/*member.Attributes.All(a => a.DataType.Name != "HideAttribute");*/
		}

		void AutoCompletePropertyOnCommit(IAutoCompleteCodeEditor editor)
		{
			editor.InsertText(editor.GetCaretOffset(), "=\"\"");
			editor.SetCarretPos(editor.GetCaretOffset() - 1);
			editor.RequestIntelliPrompt();
		}
	}
}
