using Outracks.CodeCompletion;
using Outracks.CodeCompletionFactory.UXNinja;
using Uno.Compiler;
using Outracks.UnoDevelop.CodeNinja;
using Outracks.UnoDevelop.CodeNinja.AmbientParser;
using Uno;
using CodeReader = Outracks.UnoDevelop.CodeNinja.CodeReader;

namespace Outracks.CodeCompletionFactory
{
	public class GotoDefinitionFactory : IGotoDefinitionFactory
	{
		public bool TryGotoDefinitionBasedOnSyntax(SyntaxLanguageType type, GotoDefinitionData gotoDefinitionData, out Source source)
		{
			source = null;
			var invalidType = false;

			switch (type)
			{
				case SyntaxLanguageType.Uno:
					source = GoToDefinitionUno(gotoDefinitionData);
					break;
				case SyntaxLanguageType.UX:
					source = GoToDefinitionUX(gotoDefinitionData);
					break;
				default:
					invalidType = true;
					break;
			}

			return !invalidType && source != null;
		}

		Source GoToDefinitionUX(GotoDefinitionData gotoDefinitionData)
		{
			var sourceEntity = SourceEntityFactoryUX.GetDataTypeFromOffset(
				gotoDefinitionData.FilePath,
				gotoDefinitionData.SrcCode,
				gotoDefinitionData.CaretOffset,
				gotoDefinitionData.Compiler);

			return sourceEntity == null ? null : sourceEntity.Source;
		}

		Source GoToDefinitionUno(GotoDefinitionData gotoDefinitionData)
		{
			if (gotoDefinitionData.Compiler == null)
				return null;

			var sourceEntity = GoToDefinition.TryGetSourceObject(
				gotoDefinitionData.Compiler,
				new Source(gotoDefinitionData.MainPackage, gotoDefinitionData.FilePath.NativePath),
				new CodeReader(gotoDefinitionData.SrcCode, gotoDefinitionData.CaretOffset),
				Parser.Parse(gotoDefinitionData.SrcCode));

			return sourceEntity == null ? null : sourceEntity.Source;
		}
	}
}