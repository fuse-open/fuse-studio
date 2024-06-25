using Outracks.IO;
using Uno;
using Uno.Compiler;
using Uno.Compiler.Core;

namespace Outracks.CodeCompletion
{
	public class GotoDefinitionData
	{
		public SourcePackage MainPackage { get; private set; }
		public Compiler Compiler { get; private set; }
		public AbsoluteFilePath FilePath { get; private set; }
		public string SrcCode { get; private set; }
		public int CaretOffset { get; private set; }

        public GotoDefinitionData(SourcePackage mainPackage, Compiler compiler, AbsoluteFilePath filePath, string srcCode, int caretOffset)
		{
			MainPackage = mainPackage;
			Compiler = compiler;
			FilePath = filePath;
			SrcCode = srcCode;
			CaretOffset = caretOffset;
		}
	}

	public interface IGotoDefinitionFactory
	{
		bool TryGotoDefinitionBasedOnSyntax(SyntaxLanguageType type, GotoDefinitionData gotoDefinitionData, out Source source);
	}
}