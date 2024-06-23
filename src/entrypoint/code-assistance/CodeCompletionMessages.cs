using System.Collections.Generic;
using Outracks.CodeCompletion;
using Outracks.Fuse.Protocol;

namespace Outracks.Fuse.CodeAssistance
{
	[PayloadTypeName("Fuse.GetCodeSuggestions")]
	public class GetCodeSuggestionsRequest : IRequestData<GetCodeSuggestionsResponse>
	{
		[PluginComment("Text contains all code that is present at the time of request inside the file asking for request.", "class Foo {};")]
		public string Text;

		[PluginComment("Path to the file that asks for the request.", @"C:/Test.uno")]
		public string Path;

		[PluginComment("Type of syntax, we support 'Uno' and 'UX'", "")]
		public string SyntaxType;

		[PluginComment("Caret position", "{Line: 0, Character: 0}")]
		public Protocol.Messages.TextPosition CaretPosition;
	}

	public class CodeSuggestion
	{
		[PluginComment("")]
		public string Suggestion { get; set; }

		[PluginComment("")]
		public string PreText { get; set; }

		[PluginComment("")]
		public string PostText { get; set; }

		[PluginComment("")]
		public string Type { get; set; }

		[PluginComment("")]
		public string ReturnType { get; set; }

		[PluginComment("")]
		public string[] AccessModifiers { get; set; }

		[PluginComment("")]
		public string[] FieldModifiers { get; set; }

		[PluginComment("")]
		public MethodArgument[] MethodArguments { get; set; }
	}

	[PayloadTypeName("Fuse.GetCodeSuggestions")]
	public class GetCodeSuggestionsResponse : IResponseData
	{
		[PluginComment("States if the cache is being rebuilt, so no/broken suggestions are only available")]
		public bool IsUpdatingCache;

		[PluginComment("List of suggestions")]
		public IList<CodeSuggestion> CodeSuggestions { get; set; }
	}

	[PayloadTypeName("Fuse.GotoDefinition")]
	public class GotoDefinitionRequest : IRequestData<GotoDefinitionResponse>
	{
		[PluginComment("")]
		public string Path;

		[PluginComment("")]
		public string Text;

		[PluginComment("")]
		public string SyntaxType;

		[PluginComment("")]
		public Protocol.Messages.TextPosition CaretPosition;
	}

	[PayloadTypeName("Fuse.GotoDefinition")]
	public class GotoDefinitionResponse : IResponseData
	{
		[PluginComment("")]
		public string Path;

		[PluginComment("")]
		public Protocol.Messages.TextPosition CaretPosition;
	}
}