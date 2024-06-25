using Outracks.UnoDevelop.CodeNinja.AmbientParser;
using Uno.Compiler.API.Domain.IL;

namespace Outracks.UnoDevelop.CodeNinja.CodeCompleter
{
    public partial class CodeCompleter
    {
        void SuggestNodePathTypes(string ignoredClassName = null)
        {
            foreach (var n in _context.NodePath)
            {
                if (n.Type != NodeType.Root)
                    Suggest(n.TypeOrNamespace, ignoredClassName);
                if (n.TypeOrNamespace is Namespace)
                    Suggest((n.TypeOrNamespace as Namespace).Types, ignoredClassName);
                if (n.TypeOrNamespace is DataType)
                    Suggest((n.TypeOrNamespace as DataType).NestedTypes, ignoredClassName);
            }
        }
    }
}
