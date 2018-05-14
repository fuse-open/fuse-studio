using System;
using System.Collections.Generic;
using System.Linq;
using System.Reactive.Subjects;
using Uno;

namespace Outracks.Fuse.CodeAssistanceService
{
	using CodeCompletion;
	using CodeCompletionFactory;
	using Protocol;
	using IO;

	public class CodeCompletionInstance : IDisposable
	{
		readonly IGotoDefinitionFactory _gotoDefinitionFactory;
		readonly ISuggestionsFactory _suggestionsFactory;
		readonly Engine _engine;

		public CodeCompletionInstance(ProjectDetector projectDetector)
		{
			_gotoDefinitionFactory = new GotoDefinitionFactory();
			_suggestionsFactory = new SuggestionsFactory();
			_engine = new Engine(projectDetector);
		}

		public GotoDefinitionResponse HandleGotoDefinition(GotoDefinitionRequest gotoDefinitionRequest)
		{
			// TODO: Refactor away this type of error handling...
			var errors = new Subject<Error>();			
			errors.Subscribe(e => { throw new Exception(e.Message); });

			var source = ProcessGotoDefintion(gotoDefinitionRequest, errors);
			var response = source.MatchWith(
				s => new GotoDefinitionResponse()
				{
					Path = s.FullPath,
					CaretPosition = new Protocol.Messages.TextPosition() { Line = s.Line, Character = s.Column }
				},
				() => new GotoDefinitionResponse());

			return response;
		}

		Optional<Source> ProcessGotoDefintion(GotoDefinitionRequest gotoDefinitionRequest, IObserver<Error> errors)
		{
			var path = TryCatchWithError(
				errors,
				ErrorCode.InvalidData,
				"Failed to parse path.",
				() => AbsoluteFilePath.Parse(gotoDefinitionRequest.Path));

			if (!path.HasValue)
				return Optional.None();

			Compile(gotoDefinitionRequest.Text, path.Value);

			var source = GotoDefinitionFeature.TryGoToDefinition(
				_engine,
				errors,
				_gotoDefinitionFactory,
				gotoDefinitionRequest);
			
			source.Do(s => {}, () => errors.OnNext(new Error(ErrorCode.Unknown, "Failed to goto definition.")));

			return source;
		}

		public GetCodeSuggestionsResponse HandleGetCodeSuggestions(GetCodeSuggestionsRequest request)
		{			
			// TODO: Refactor away this type of error handling...
			var errors = new Subject<Error>();
			errors.Subscribe(e => { throw new Exception(e.Message); });

			var suggestItems = ProcessCodeCompletion(request, errors);
			var response = new GetCodeSuggestionsResponse()
			{
				IsUpdatingCache = _engine.Compiler == null,
				CodeSuggestions =
					Enumerable.ToList<CodeSuggestion>(suggestItems.Select(
							s => new CodeSuggestion()
							{
								Suggestion = s.Text,
								PreText = s.AutoCompletePreText == null ? "" : s.AutoCompletePreText(),
								PostText = s.AutoCompletePostText == null ? "" : s.AutoCompletePostText(),
								Type = s.Type.ToString(),
								ReturnType = s.AutoCompleteDescriptionText == null ? "" : s.AutoCompleteDescriptionText(),
								AccessModifiers = s.AutoCompleteAccessModifiers ?? new string[0],
								FieldModifiers = s.AutoCompleteFieldModifiers ?? new string[0],
								MethodArguments = s.AutoCompleteMethodArguments ?? new MethodArgument[0]
							}))
			};

			return response;
		}

		IEnumerable<SuggestItem> ProcessCodeCompletion(GetCodeSuggestionsRequest request, IObserver<Error> errors)
		{
			IList<SuggestItem> suggestItems = new List<SuggestItem>();
			var path = TryCatchWithError(errors, ErrorCode.InvalidData, "Failed to parse path.", () => AbsoluteFilePath.Parse(request.Path));
			
			if (!path.HasValue)
				return suggestItems;			

			try
			{
				Compile(request.Text, path.Value);

				SuggestionsFeature.TryGetSuggestions(
					errors,
					_engine,
					_suggestionsFactory,
					request,
					out suggestItems);
			}
			catch (Exception e)
			{
				errors.OnNext(new Error(ErrorCode.InvalidOperation, e.Message));
			}

			return suggestItems;
		}

		void Compile(string source, AbsoluteFilePath filePath)
		{
			var logger = new Logger();
			if (!_engine.Compile(logger, source, Optional.Some(filePath)))
				return;
		}

		static Optional<T> TryCatchWithError<T>(IObserver<Error> errors, ErrorCode code, string message, Func<T> func)
		{
			try
			{
				return func();
			}
			catch (Exception)
			{
				errors.OnNext(new Error(code, message));
				return Optional.None();
			}
		}

		public void Dispose()
		{
			_engine.Dispose();
		}
	}
}