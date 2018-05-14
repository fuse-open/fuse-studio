using System.Linq;
using Outracks.CodeCompletion;
using Uno;
using Uno.Compiler.API.Domain;
using Uno.Compiler.API.Domain.IL;
using Uno.Compiler.API.Domain.IL.Types;
using Uno.Compiler.Core.Syntax.Compilers;
using Uno.Compiler.Core.Syntax.Binding;
using Uno.Compiler.Frontend.Analysis;
using Uno.Logging;

namespace Outracks.UnoDevelop.CodeNinja.CodeCompleter
{
    public partial class CodeCompleter
    {
        bool InCatchBlockHeader()
        {
            if (_context.NodePath.Count == 0) return false;

            // If we are in a catch block before the first curly brace
            if (Enumerable.Last(_context.NodePath).Type == AmbientParser.NodeType.Catch)
            {
                var oldOffset = _reader.Offset;
                _reader.Offset = Enumerable.Last(_context.NodePath).StartOffset;
                while (true)
                {
                    var t = _reader.ReadToken();
                    if (t == TokenType.LeftCurlyBrace) break;
                    if (_reader.Offset > oldOffset)
                    {
                        _reader.Offset = oldOffset;
                        return true;
                    }
                    if (t == TokenType.EndOfFile) break;
                }
            }
            return false;
        }

        // ICKKKKKKK
        bool _exceptionTypesOnly;

        enum SuggestTypesMode
        {
            Importers,
            Blocks,
            Attributes,
            Everything
        }

        void SuggestTypes(string memberExp, bool includeStaticMembers, bool metaContext, SuggestTypesMode mode = SuggestTypesMode.Everything, bool onlyClassTypes = false)
        {
            _exceptionTypesOnly = InCatchBlockHeader();

            SourceObject item = _context.TypeOrNamespace;

            // Suggest generic arguments
            if (mode == SuggestTypesMode.Everything)
            {
                var vdt = _context.TypeOrNamespace as DataType;
                while (vdt != null)
                {
                    if (vdt.GenericParameters != null)
                        foreach (var t in vdt.GenericParameters) Suggest(t);
                    vdt = vdt.Parent as DataType;
                }

            }

            if (memberExp == "")
            {
                foreach (var u in _context.Usings)
                {
                    var ns = _context.Compiler.ILFactory.GetEntity(_context.Root.Source, u, _context.Root) as Namespace;
                    if (ns != null)
                    {
                        foreach(var i in ns.Types)
                        {
                            if (!onlyClassTypes || (i.TypeType == TypeType.Class || i.TypeType == TypeType.Interface))
                                Suggest(i, null, mode);
                        }
                        if (mode == SuggestTypesMode.Blocks) Suggest(ns.Blocks);
                    }
                }

                if (mode == SuggestTypesMode.Everything)
                {
                    FunctionCompiler fc = null;
                    foreach (var us in _context.UsingStatics)
                    {
                        var dte = Parser.ParseExpression(_compiler.Log, _source, us, metaContext ? ParseContext.MetaProperty : ParseContext.Default);
                        if (!dte.IsInvalid)
                        {
                            if (fc == null) fc = CreateFunctionCompiler(_methodNode);
                            TrySuggestMembers(fc, dte, true);
                        }
                    }

                    SuggestNodePathTypes();
                }
            }

            if (memberExp != "")
            {
                item = ResolveStringInFunctionContext(_methodNode, memberExp, _source, new FunctionCompiler(_context.Compiler, _context.TypeOrNamespace));
                if (mode == SuggestTypesMode.Everything && memberExp.EndsWith("<")) SuggestKeywords(TypeAliases.AllAliases.ToArray());

                if (item is PartialNamespace)
                {
                    var ns = (item as PartialNamespace).Namespace;
                    Suggest(ns.Namespaces, null, mode);
                    Suggest(ns.Types, null, mode);

                    if (mode == SuggestTypesMode.Blocks) Suggest(ns.Blocks);
                }

                if (includeStaticMembers && item is PartialType)
                {
                    var dt = (item as PartialType).Type;
                    SuggestTypeMembers(dt, AccessorLevel.Public, true, metaContext, false, null);
                }

            }
            else
            {
                if (mode == SuggestTypesMode.Everything && !_exceptionTypesOnly)
                {
                    SuggestKeywords(TypeAliases.AllAliases.ToArray());
                    SuggestKeywords("void");
                }

                while (item != null)
                {

                    if (item is Namespace)
                    {
                        var ns = item as Namespace;
                        Suggest(ns.Namespaces, null, mode);
                        Suggest(ns.Types, null, mode);

                        if (mode == SuggestTypesMode.Blocks) Suggest(ns.Blocks);

                        item = ns.Parent;
                    }
                    else if (includeStaticMembers && item is DataType)
                    {
                        var dt = item as DataType;
                        SuggestTypeMembers(dt, AccessorLevel.Private, true, metaContext, false, null);

                        item = dt.Parent;
                        metaContext = false;
                    }
                    else if (item is DataType)
                    {
                        var dt = item as DataType;
                        item = dt.Parent;
                        if (dt.Block != null && mode == SuggestTypesMode.Blocks) Suggest(dt.Block.NestedBlocks);
                    }
                    else break;
                }
            }
        }

        void SuggestLocals(FunctionCompiler func)
        {
            foreach (var vs in func.VariableScopeStack)
            {
                foreach (var v in vs.Variables)
                    Suggest(SuggestItemType.Variable, v.Value, v.Value.Name);
            }

            foreach (var p in func.Function.Parameters)
                Suggest(SuggestItemType.MethodArgument, p, p.Name);
        }


    }
}
