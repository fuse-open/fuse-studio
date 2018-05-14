using System.Collections.Generic;
using System.IO;
using System.Linq;
using Outracks.CodeCompletion;
using Uno.Compiler.Core.Syntax.Compilers;
using Uno.Compiler.Frontend.Analysis;
using Uno.Logging;
using Outracks.UnoDevelop.CodeNinja.AmbientParser;
using Uno;
using Uno.Compiler.API.Domain;
using Uno.Compiler.API.Domain.Graphics;
using Uno.Compiler.API.Domain.IL.Expressions;
using Uno.Compiler.API.Domain.IL.Members;
using Uno.Compiler.API.Domain.IL.Types;
using Parser = Uno.Compiler.Frontend.Analysis.Parser;

namespace Outracks.UnoDevelop.CodeNinja.CodeCompleter
{
    public partial class CodeCompleter
    {
        void SuggestBlock()
        {
            TokenType tt;
            var memberExp = FindMemberExpression(out tt, true);

            // Build block
            var methodCompiler = CreateFunctionCompiler(_methodNode);
            _compiler.BlockBuilder.Build();

            var dte = Parser.ParseExpression(_compiler.Log, _source, memberExp, _context.MetaPropertyNode != null ? ParseContext.MetaProperty : ParseContext.Default);
	        
            if (dte.IsInvalid)
	        {
				SuggestForBlock(methodCompiler, memberExp);   
	        }            

            if (_context.MetaPropertyNode != null)
            {
                SuggestForMetaproperty(methodCompiler, memberExp);
            }
        }

        void SuggestForBlock(FunctionCompiler methodCompiler, string memberExp)
        {
            if (memberExp == "")
            {
                var last = Enumerable.Last(_context.NodePath);

                SuggestKeywordsIfCan(last);
                SuggestKeywords(TypeAliases.AllAliases.ToArray());

                var lastInlineBlock = _context.InlineBlock;
                var lastBlock = _context.Block;
                if (lastBlock != null && lastBlock.BlockBase is Block)
                {
                    var b = (Block)lastBlock.BlockBase;
                    SuggestBlockItems(b);
                }
                else if (lastInlineBlock != null && lastInlineBlock.Children != null)
                {
                    SuggestForInlineBlock(methodCompiler, lastInlineBlock);
                }
            }

            SuggestTerminals();
            SuggestKeywords("vertex_attrib", "sample", "import", "req", "tag");

            SuggestTypes(memberExp, true, true);
        }

        void SuggestForInlineBlock(FunctionCompiler methodCompiler, AmbientParser.Node lastInlineBlock)
        {
            foreach (var c in lastInlineBlock.Children)
            {
                switch (c.Type)
                {
                    case NodeType.Apply:
                        SuggestForApplyBlockItems(c);
                        break;
                    case NodeType.MetaProperty:
                        SuggestMetaproperty(methodCompiler, c);
                        break;
                }
            }
        }

        void SuggestMetaproperty(FunctionCompiler methodCompiler, AmbientParser.Node c)
        {
            var mp = c.MetaProperty ?? ResolveMetaproperty(methodCompiler, c);
            if (mp == null) return;

            var m = new GetMetaProperty(_source,
                                            mp.ReturnType,
                                            c.Name);
            Suggest(SuggestItemType.MetaProperty, m, m.Name);
        }

        void SuggestForApplyBlockItems(AmbientParser.Node c)
        {
            var log = new Log(new StringWriter());
            var dte = Parser.ParseExpression(log, 
                                             _source,
                                             c.Name,
                                             ParseContext.MetaProperty);
            if (log.ErrorCount != 0) return;
            var b = _context.Compiler.NameResolver.GetBlock(
                                                   _context.TypeOrNamespace as
                                                   ClassType,
                                                   dte);
            if (b != null) SuggestBlockItems(b);
        }

        void SuggestForMetaproperty(FunctionCompiler methodCompiler, string memberExp)
        {
            var mpn = _context.MetaPropertyNode;

            var mp = mpn.MetaProperty;
            if (mp == null)
            {
                mp = ResolveMetaproperty(methodCompiler, mpn) ?? new MetaProperty(Source.Unknown,
                                                                                _context.BlockBase,
                                                                                _context.Compiler.ILFactory.Essentials.Object,
                                                                                mpn.Name,
                                                                                0);
            }

            var fc = new FunctionCompiler(_compiler, mp);

            // Parse the member expression
            var dte = Parser.ParseExpression(_compiler.Log, _source, memberExp, ParseContext.MetaProperty);

            if (memberExp == "")
            {
                if (!methodCompiler.Function.IsStatic) SuggestKeywords("this");
                SuggestKeywords("debug_log",
                                "var",
                                "for",
                                "pixel",
                                "prev",
                                "foreach",
                                "case",
                                "default",
                                "break",
                                "if",
                                "while",
                                "do",
                                "switch",
                                "try",
                                "else",
                                "catch",
                                "throw",
                                "draw",
                                "return",
                                "assert");

                // Suggest static members
                foreach (string u in _context.Usings)
                {
                    TrySuggestMembers(methodCompiler,
                                      Parser.ParseExpression(_compiler.Log, _source, u, ParseContext.MetaProperty),
                                      false);
                }
            }

            if (!dte.IsInvalid)
            {
                TrySuggestMembers(fc, dte, true);
                TrySuggestMembers(methodCompiler, dte, true);
            }
            else
                SuggestLocals(methodCompiler);
        }


        void SuggestKeywordsIfCan(AmbientParser.Node last)
        {
            var metapropertyDef = last.Type == NodeType.MetaPropertyDefinitionScope ||
                last.Type == NodeType.MetaPropertyDefinition;

            var childrenIsNotMetapropertyDef = last.Children == null ||
                 last.Children.LastOrDefault(x => x.Type == NodeType.MetaPropertyDefinition) == null;

            if (!metapropertyDef &&
                childrenIsNotMetapropertyDef)
            {
                SuggestKeywords("drawable", "pixel", "vertex", "volatile", "init", "apply");
            }

            if (last.Type == NodeType.MetaProperty)
            {
                SuggestKeywords("is", "as");
            }
        }

        static MetaProperty ResolveMetaproperty(FunctionCompiler methodCompiler, AmbientParser.Node metaPropertyNode)
        {
            MetaProperty mp;
            var function = methodCompiler.Function as Method;
            if (function == null) return null;
            foreach (var block in function.DrawBlocks)
            {
                mp = ResolveForEachMetaProperty(block.Members, metaPropertyNode);
                if (mp != null) return mp;

                foreach (var item in block.Members)
                {
                    var apply = item as Apply;
                    if (apply != null && apply.Block != null)
                    {
                        mp = ResolveForEachMetaProperty(apply.Block.Members, metaPropertyNode);
                        if (mp != null) return mp;
                    }
                }
            }

            return null;
        }

        static MetaProperty ResolveForEachMetaProperty(IEnumerable<BlockMember> blockItems, AmbientParser.Node metaPropertyNode)
        {
            return blockItems.OfType<MetaProperty>()
                .FirstOrDefault(
                    metaproperty => metaPropertyNode.StartOffset >= metaproperty.Source.Offset 
                    && metaproperty.Name == metaPropertyNode.Name
                );
        }
    }
}
