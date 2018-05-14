using Uno.Compiler.Frontend.Analysis;

namespace Outracks.UnoDevelop.CodeNinja.CodeCompleter
{
    public partial class CodeCompleter
    {
        void SuggestImports()
        {
            TokenType t;
            SuggestTypes(FindMemberExpression(out t, _context.InTypeBody), false, false, SuggestTypesMode.Importers);
        }

        void SuggestBlockFactories()
        {
            TokenType t;
            SuggestTypes(FindMemberExpression(out t, _context.InTypeBody), false, false, SuggestTypesMode.Blocks);
        }
    }
}
