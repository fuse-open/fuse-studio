using System.Linq;
using Uno.Compiler.Frontend.Analysis;
using Uno.Logging;

namespace Outracks.UnoDevelop.CodeNinja.CodeCompleter
{
    public partial class CodeCompleter
    {
        void ComputeDrawstatementSuggestions()
        {
            TokenType tt;
            var memberExp = FindMemberExpression(out tt, true);

            if (memberExp == "")
            {
                var funcCompiler = CreateFunctionCompiler(_methodNode);

                if (!funcCompiler.Function.IsStatic) SuggestKeywords("this");

                SuggestUsingOrApply("", true, true);
                SuggestLocals(funcCompiler);
                return;
            }
            
            var dte = Parser.ParseExpression(_compiler.Log, _source, memberExp);
            var methodCompiler = CreateFunctionCompiler(_methodNode);
            
            if (!dte.IsInvalid)
            {
                TrySuggestMembers(methodCompiler, dte, true);
            }
        }
    }
}
