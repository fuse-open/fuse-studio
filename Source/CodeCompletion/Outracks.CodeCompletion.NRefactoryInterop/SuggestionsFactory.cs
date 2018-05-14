using System.Collections.Generic;
using System.Linq;
using Outracks.CodeCompletion;
using Outracks.CodeCompletionFactory.UXNinja;
using Uno.Compiler;
using Uno.Compiler.Core;
using Outracks.UnoDevelop.CodeNinja;
using Outracks.UnoDevelop.CodeNinja.AmbientParser;
using Outracks.UnoDevelop.CodeNinja.CodeCompleter;
using Uno;
using CodeReader = Outracks.UnoDevelop.CodeNinja.CodeReader;

namespace Outracks.CodeCompletionFactory
{
	public class SuggestionsFactory : ISuggestionsFactory
	{
		public bool SuggestionsBasedOnSyntax(SyntaxLanguageType type, SuggestionDependentData suggestionData, out IList<SuggestItem> suggestItems)
		{
			suggestItems = null;
			var invalidType = false;

			switch (type)
			{
				case SyntaxLanguageType.Uno:
					suggestItems = SuggestionsUno(suggestionData);
					break;
				case SyntaxLanguageType.UX:
					suggestItems = SuggestionsUX(suggestionData);
					break;
				default:
					invalidType = true;
					break;
			}

			return !invalidType;
		}

		IList<SuggestItem> SuggestionsUX(SuggestionDependentData suggestionData)
		{
            var suggestions = CreateUXSuggestions.Do(suggestionData.Compiler,
                suggestionData.FilePath,
                suggestionData.SrcCode,
                suggestionData.CaretOffset);

            var suggestionsList = suggestions.GroupBy(s => s.Text).Select(g => g.First()).OrderByDescending(s => s.Priority).ToList();
                
            return suggestionsList;
		}

		IList<SuggestItem> SuggestionsUno(SuggestionDependentData suggestionData)
		{
			var suggestions = ResolveSuggestions(suggestionData);
			suggestions = suggestions as IList<SuggestItem> ?? suggestions.ToList();
            return suggestions.OrderBy(s => s.Text).OrderByDescending(s => s.Priority).ToList();
		}

		IEnumerable<SuggestItem> ResolveSuggestions(SuggestionDependentData suggestionData)
		{
			if (suggestionData.Compiler == null)
				return Enumerable.Empty<SuggestItem>();

			var codeCompleter = new CodeCompleter(suggestionData.Compiler,
				new Source(suggestionData.MainPackage, suggestionData.FilePath.NativePath),
				new CodeReader(suggestionData.SrcCode, suggestionData.CaretOffset),
				suggestionData.CaretOffset,
				Parser.Parse(suggestionData.SrcCode));

			ConfidenceLevel level;
			var suggestions = codeCompleter.SuggestCompletion("", out level);
			return suggestions;
		}
	}
}