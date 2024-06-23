using System.Collections.Generic;
using System.Linq;
using Outracks.CodeCompletion;
using Outracks.IO;
using Outracks.UnoDevelop.UXNinja;
using Uno.Compiler.API;

namespace Outracks.CodeCompletionFactory.UXNinja
{
	class CreateUXSuggestions
	{
		public static IEnumerable<SuggestItem> Do(ICompiler compiler, AbsoluteFilePath filePath, string code, int caret)
		{
			return new CreateUXSuggestions().CreateSuggestionsInternal(compiler, filePath, code, caret);
		}

		IEnumerable<SuggestItem> CreateSuggestionsInternal(ICompiler compiler, AbsoluteFilePath file, string code, int caret)
		{
			if (compiler == null)
				return Enumerable.Empty<SuggestItem>();

			var suggestions = SuggestionParser.GetSuggestions(
				compiler,
				Context.CreateContext(
					file,
					code,
					caret,
					compiler.Input.Package),
				caret,
				new CodeReader());

			return suggestions;
		}
	}
}