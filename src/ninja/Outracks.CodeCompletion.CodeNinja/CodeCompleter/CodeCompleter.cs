using System.Collections.Generic;
using System.Linq;
using Outracks.CodeCompletion;
using Outracks.UnoDevelop.CodeNinja.AmbientParser;
using Uno;
using Uno.Compiler.API.Domain;
using Uno.Compiler.API.Domain.IL;
using Uno.Compiler.API.Domain.IL.Members;
using Uno.Compiler.Core;
using Uno.Compiler.Frontend.Analysis;

namespace Outracks.UnoDevelop.CodeNinja.CodeCompleter
{
    public enum ConfidenceLevel
    {
        Exact,
        Faulty,
        Failed
    }

    public partial class CodeCompleter
    {
		readonly Compiler _compiler;
        readonly IUnoCodeReader _reader;
        readonly Context _context;
        readonly Source _source;
        Node _methodNode;

        public Context Context { get { return _context; } }

        public CodeCompleter(Compiler compiler, Source src, IUnoCodeReader codeReader, int caret, ParseResult parseResult)
        {
            _compiler = compiler;
            _reader = codeReader;
            _source = src;
            _context = new Context(_compiler, src, parseResult, caret, codeReader.Length);
        }

        public IEnumerable<SuggestItem> SuggestCompletion(string lastTextEntered, out ConfidenceLevel confidence)
        {
            confidence = ConfidenceLevel.Exact;
            if (_compiler == null) return new List<SuggestItem>();

            _suggestions = new List<SuggestItem>();
            confidence = ComputeSuggestions(lastTextEntered);

			if (_compiler == null) confidence = ConfidenceLevel.Faulty;

            return _suggestions;
        }

        ConfidenceLevel ComputeSuggestions(string lastTextEntered)
        {
            if (InNameDeclarationOrOperatorPosition()) return ConfidenceLevel.Exact;

            _methodNode = _context.NodePath.LastOrDefault(x => NodeTypeHelpers.IsMethodNode(x.Type));

            ConfidenceLevel conf = ComputeQuickWins();
            if (conf != ConfidenceLevel.Failed) return conf;

            if (lastTextEntered == " ") return ConfidenceLevel.Exact;

            if (_context.NodePath.Count < 2)
            {
                SuggestRootItems();
                return ConfidenceLevel.Exact;
            }

            switch (Enumerable.Last(_context.NodePath).Type)
            {
                case NodeType.Block:
                case NodeType.MetaProperty:
                case NodeType.MetaPropertyDefinition:
                    {
                        SuggestBlock();
                        return ConfidenceLevel.Faulty;
                    }

                case NodeType.Apply:
                    {
                        SuggestUsingOrApply(_reader.ReadBackwardsTo(Tokens.Apply).Trim(), true, true);
                        return ConfidenceLevel.Exact;
                    }

                case NodeType.Using:
                    {
                        SuggestUsingOrApply(_reader.ReadBackwardsTo(TokenType.Using, TokenType.Comma).Trim());
                        return ConfidenceLevel.Exact;
                    }

                case NodeType.Class:
                case NodeType.Struct:
                    {
                        ComputeTypeBodySuggestions();
                        return ConfidenceLevel.Faulty;
                    }

                case NodeType.Method:
                case NodeType.Catch:
                case NodeType.GetScope:
                case NodeType.AddScope:
                case NodeType.RemoveScope:
                case NodeType.MetaPropertyDefinitionScope:
                    {
                        ComputeFunctionBodySuggestions();
                        return ConfidenceLevel.Exact;
                    }
                case NodeType.DrawStatement:
                    {
                        ComputeDrawstatementSuggestions();
                        return ConfidenceLevel.Exact;
                    }

                case NodeType.Field:
                    {
                        ComputeFunctionBodySuggestions(true);
                        return ConfidenceLevel.Exact;
                    }

                case NodeType.Property:
                case NodeType.Indexer:
                    {
                        var p = Enumerable.Last(_context.NodePath);

                        bool hasGet = false;
                        bool hasSet = false;
                        if (p.Children != null)
                        {
                            foreach (var c in p.Children)
                            {
                                if (c.Type == NodeType.GetScope) hasGet = true;
                                if (c.Type == NodeType.SetScope) hasSet = true;
                            }
                        }
                        SuggestKeywords("public", "protected", "private", "internal");
                        if (!hasGet) SuggestKeywords("get");
                        if (!hasSet) SuggestKeywords("set");
                        return ConfidenceLevel.Exact;
                    }

                case NodeType.SetScope:
                    {
                        ComputeFunctionBodySuggestions();
                        SuggestKeywords("value");
                        return ConfidenceLevel.Exact;
                    }

                case NodeType.Operator:
                    {
                        ComputeFunctionBodySuggestions();
                        return ConfidenceLevel.Exact;
                    }

                case NodeType.Constructor:
                    {
                        ComputeFunctionBodySuggestions();
                        return ConfidenceLevel.Exact;
                    }

                case NodeType.Root:
                case NodeType.Namespace:
                    {
                        SuggestRootItems();
                        return ConfidenceLevel.Exact;
                    }

                case NodeType.InlineBlock:
                    {
                        SuggestBlock();
                        return ConfidenceLevel.Faulty;
                    }
            }

            return ConfidenceLevel.Faulty;
        }

        bool IsMemberAccessible(Modifiers memberModifiers, DataType memberDeclType, DataType accessingType)
        {
            if (memberModifiers.HasFlag(Modifiers.Public)) return true;

            if (accessingType.Equals(memberDeclType)) return true;

            if (memberModifiers.HasFlag(Modifiers.Protected))
            {
                if (accessingType.IsSubclassOf(memberDeclType)) return true;
            }

            return accessingType.IsChildClassOf(memberDeclType);
        }

        bool IsMemberAccessible(Member member, DataType accessingType)
        {
            return IsMemberAccessible(member.Modifiers, member.DeclaringType, accessingType);
        }
    }

    public static class ICodeReaderHelpers
    {
        public static string ReadBackwardsTo(this IUnoCodeReader r, params TokenType[] tokenTypes)
        {
            var offset = r.Offset;
            while (true)
            {
                var tt = r.PeekTokenReverse();
                if (tt == TokenType.EndOfFile)
                {
                    return r.ReadText(offset - r.Offset);
                }
                if (tokenTypes.Contains(tt))
                {
                    return r.ReadText(offset - r.Offset);
                }
                var k = r.ReadTokenReverse();
                if (k == TokenType.EndOfFile) return null;
            }
        }

        public static string ReadBackwardsTo(this IUnoCodeReader r, string token)
        {
            var offset = r.Offset;
            while (true)
            {
                var tt = r.PeekTokenReverse();
                if (tt == TokenType.EndOfFile)
                {
                    return r.ReadText(offset - r.Offset);
                }
                if (tt == TokenType.Identifier)
                {
                    var text = r.ReadText(offset - r.Offset);
                    if (text.StartsWith(token))
                        return text;
                }
                var k = r.ReadTokenReverse();
                if (k == TokenType.EndOfFile) return null;
            }
        }
    }
}
