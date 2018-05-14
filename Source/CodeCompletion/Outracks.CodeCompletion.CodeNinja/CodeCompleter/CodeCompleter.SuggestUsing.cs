using System.Linq;
using Uno.Compiler.API.Domain.IL;
using Uno.Compiler.API.Domain.IL.Types;

namespace Outracks.UnoDevelop.CodeNinja.CodeCompleter
{
    public partial class CodeCompleter
    {
        void SuggestUsingOrApply(string qualifier, bool includeTypes = false, bool includeBlocks = false)
        {
            if (!qualifier.Contains('.'))
            {
                if (includeTypes) SuggestTypes("", false, false, SuggestTypesMode.Everything);
                if (includeBlocks) SuggestTypes("", false, false, SuggestTypesMode.Blocks);
            }
            else
            {
                var p = qualifier.Split('.');

                Namescope ns = _context.Root;
                foreach (var pp in p.Take(p.Length - 1))
                {
                    if (ns is Namespace)
                    {
                        foreach (var nn in (ns as Namespace).Namespaces)
                        {
                            if (nn.Name == pp)
                            {
                                ns = nn;
                                break;
                            }
                        }
                        if (includeTypes)
                        {
                            foreach (var nn in (ns as Namespace).Types)
                            {
                                if (nn.Name == pp)
                                {
                                    ns = nn;
                                    break;
                                }
                            }
                        }
                    }
                    if (includeTypes && ns is DataType)
                    {
                        foreach (var nn in (ns as DataType).NestedTypes)
                        {
                            if (nn.Name == pp)
                            {
                                ns = nn;
                                break;
                            }
                        }
                    }
                }

                if (ns is Namespace)
                {
                    Suggest((ns as Namespace).Namespaces);

                    if (includeTypes)
                    {
                        Suggest((ns as Namespace).Types);
                    }

                    if (includeBlocks)
                    {
                        Suggest((ns as Namespace).Blocks);
                    }
                }
                else if (includeTypes && ns is DataType)
                {
                    Suggest((ns as DataType).NestedTypes);

                    if (includeBlocks)
                    {
                        Suggest((ns as ClassType).Block.NestedBlocks);
                    }
                }
            }
        }
    }
}
