using System;
using System.Collections.Generic;
using Outracks.CodeCompletion;
using Outracks.Fuse.Protocol;
using Outracks.IO;

namespace Outracks.Fuse.CodeAssistance
{
	class SuggestionsFeature
	{
		readonly IObserver<Error> _errors;
		readonly IEngine _engine;
		readonly ISuggestionsFactory _suggestionFactory;

		SuggestionsFeature(IObserver<Error> errors, IEngine engine, ISuggestionsFactory suggestionFactory)
		{
			_errors = errors;
			_engine = engine;
			_suggestionFactory = suggestionFactory;
		}

		public static bool TryGetSuggestions(IObserver<Error> errors, IEngine engine, ISuggestionsFactory suggestionFactory, GetCodeSuggestionsRequest codeCompletion, out IList<SuggestItem> suggestItems)
		{
			return new SuggestionsFeature(errors, engine, suggestionFactory).SuggestionsBasedOnSyntax(codeCompletion, out suggestItems);
		}

		bool SuggestionsBasedOnSyntax(GetCodeSuggestionsRequest codeCompletion, out IList<SuggestItem> suggestItems)
		{
			suggestItems = new List<SuggestItem>();

			if (!ValidateData(codeCompletion))
				return false;

			var textPosition = new TextPosition(
				new LineNumber(codeCompletion.CaretPosition.Line),
				new CharacterNumber(codeCompletion.CaretPosition.Character));

			var syntax = SyntaxLanguage.FromString(codeCompletion.SyntaxType).Syntax;
			var normalizedText = codeCompletion.Text.NormalizeLineEndings();
			var success = _suggestionFactory.SuggestionsBasedOnSyntax(
				syntax,
				new SuggestionDependentData(
					_engine.MainPackage,
					_engine.Compiler,
					AbsoluteFilePath.Parse(codeCompletion.Path),
					normalizedText,
					textPosition.ToOffset(normalizedText)),
					out suggestItems);

			if (success)
				return true;

			_errors.OnNext(new Error(ErrorCode.InvalidData, string.Format("Tried to create suggestions for unknown syntax type {0}", new[] { codeCompletion.SyntaxType })));

			return false;
		}

		bool ValidateData(GetCodeSuggestionsRequest codeCompletion)
		{
			var valid = true;

			if (codeCompletion.CaretPosition.Line <= 0 || codeCompletion.CaretPosition.Character <= 0)
			{
				_errors.OnNext(new Error(ErrorCode.InvalidData, "The caret offset was invalid in code completion request."));
				valid = false;
			}
			else if (string.IsNullOrEmpty(codeCompletion.Text))
			{
				_errors.OnNext(new Error(ErrorCode.InvalidData, "Text was empty or null in code completion request."));
				valid = false;
			}

			return valid;
		}
	}
}