using System.Collections.Generic;
using Outracks.CodeCompletion;

namespace Outracks.UnoDevelop.UXNinja
{
	class EndTagNameSuggestion : ISuggestion
	{
		readonly IElementContext _lastElement;

		public EndTagNameSuggestion(IElementContext lastElement)
		{
			_lastElement = lastElement;
		}

		public IEnumerable<SuggestItem> Suggest()
		{
			if (_lastElement == null) yield break;

			yield return SuggestionHelper.Suggest(SuggestItemType.Property, null, _lastElement.Name, null, null, AutoCompletePropertyOnCommit);
		}

		void AutoCompletePropertyOnCommit(IAutoCompleteCodeEditor autoCompleteCodeEditor)
		{
			autoCompleteCodeEditor.InsertText(autoCompleteCodeEditor.GetCaretOffset(), ">");
		}
	}
}