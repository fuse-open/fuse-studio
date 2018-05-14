using System;
using Outracks.CodeCompletion;
using Outracks.Fuse.Protocol;
using Outracks.IO;
using Uno;
using Uno.Compiler;

namespace Outracks.Fuse.CodeAssistanceService
{
	class GotoDefinitionFeature 
	{
		readonly IEngine _engine;
		readonly IObserver<Error> _errors;
		readonly IGotoDefinitionFactory _gotoDefinitionFactory;

		GotoDefinitionFeature(IEngine engine, IObserver<Error> errors, IGotoDefinitionFactory gotoDefinitionFactory)
		{
			_engine = engine;
			_errors = errors;
			_gotoDefinitionFactory = gotoDefinitionFactory;
		}

		public static Optional<Source> TryGoToDefinition(IEngine engine, IObserver<Error> errors, IGotoDefinitionFactory gotoDefinitionFactory,
			GotoDefinitionRequest goToDefinition)
		{
			return new GotoDefinitionFeature(engine, errors, gotoDefinitionFactory)
				.TryGoToDefinitionBasedOnType(goToDefinition);
		}

		Optional<Source> TryGoToDefinitionBasedOnType(GotoDefinitionRequest goToDefinition)
		{
			if (!ValidateData(goToDefinition))
				return Optional.None();

			if (_engine.Compiler == null)
				return Optional.None();

			var textPosition = new TextPosition(
				new LineNumber(goToDefinition.CaretPosition.Line), 
				new CharacterNumber(goToDefinition.CaretPosition.Character));

			Source source;
			var syntax = SyntaxLanguage.FromString(goToDefinition.SyntaxType).Syntax;
			var normalizedText = goToDefinition.Text.NormalizeLineEndings();
			var success = _gotoDefinitionFactory.TryGotoDefinitionBasedOnSyntax(
				syntax,
				new GotoDefinitionData(
					_engine.MainPackage,
					_engine.Compiler,
					AbsoluteFilePath.Parse(goToDefinition.Path),
					normalizedText,
					textPosition.ToOffset(normalizedText)),
				out source);

			if (success)
				return source;

			_errors.OnNext(new Error(ErrorCode.InvalidData, string.Format("Tried to go to definition in unknown syntax code {0}", goToDefinition.SyntaxType )));

			return Optional.None();
		}

		bool ValidateData(GotoDefinitionRequest goToDefinition)
		{
			var valid = true;
			if (goToDefinition.CaretPosition == null || 
				goToDefinition.CaretPosition.Line <= 0 || 
				goToDefinition.CaretPosition.Character <= 0)
			{
				_errors.OnNext(new Error(ErrorCode.InvalidData, "The caret offset was invalid in code completion request."));
				valid = false;
			}
			else if (string.IsNullOrEmpty(goToDefinition.Text))
			{
				_errors.OnNext(new Error(ErrorCode.InvalidData, "Text was empty or null in code completion request."));
				valid = false;
			}

			return valid;
		}
	}
}