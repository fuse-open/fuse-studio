using System.Collections.Generic;
using Outracks.IO;
using Uno.Compiler;
using Uno.Compiler.Core;

namespace Outracks.CodeCompletion
{
	public class SuggestionDependentData
	{
		public SourcePackage MainPackage { get; private set; }
        public Compiler Compiler { get; private set; }
		public AbsoluteFilePath FilePath { get; private set; }
		public string SrcCode { get; private set; }
		public int CaretOffset { get; private set; }

        public SuggestionDependentData(SourcePackage mainPackage, Compiler compiler, AbsoluteFilePath filePath, string srcCode, int caretOffset)
		{
			MainPackage = mainPackage;
			Compiler = compiler;
			FilePath = filePath;
			SrcCode = srcCode;
			CaretOffset = caretOffset;
		}
	}

	public interface ISuggestionsFactory
	{
		 bool SuggestionsBasedOnSyntax(SyntaxLanguageType type, SuggestionDependentData suggestionData, out IList<SuggestItem> suggestItems);
	}
}