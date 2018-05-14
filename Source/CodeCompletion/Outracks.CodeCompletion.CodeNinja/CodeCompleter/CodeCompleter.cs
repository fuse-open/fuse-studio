using System.Collections.Generic;
using System.Linq;
using Outracks.CodeCompletion;
using Uno.Compiler.Core;
using Uno.Compiler.Frontend.Analysis;
using Outracks.UnoDevelop.CodeNinja.AmbientParser;
using Uno;
using Uno.Compiler.API.Domain;
using Uno.Compiler.API.Domain.IL;
using Uno.Compiler.API.Domain.IL.Members;

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

        public AmbientParser.Context Context { get { return _context; } }

        public CodeCompleter(Compiler compiler, Source src, IUnoCodeReader codeReader, int caret, AmbientParser.ParseResult parseResult)
        {
            _compiler = compiler;
            _reader = codeReader;
            _source = src;
            _context = new AmbientParser.Context(_compiler, src, parseResult, caret, codeReader.Length);
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

            _methodNode = _context.NodePath.LastOrDefault(x => AmbientParser.NodeTypeHelpers.IsMethodNode(x.Type));

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
                case AmbientParser.NodeType.Block:
                case AmbientParser.NodeType.MetaProperty:
                case AmbientParser.NodeType.MetaPropertyDefinition:
                    {
                        SuggestBlock();
                        return ConfidenceLevel.Faulty;
                    }

                case AmbientParser.NodeType.Apply:
                    {
                        SuggestUsingOrApply(_reader.ReadBackwardsTo(Tokens.Apply).Trim(), true, true);
                        return ConfidenceLevel.Exact;
                    }

                case AmbientParser.NodeType.Using:
                    {
                        SuggestUsingOrApply(_reader.ReadBackwardsTo(TokenType.Using, TokenType.Comma).Trim());
                        return ConfidenceLevel.Exact;
                    }

                case AmbientParser.NodeType.Class:
                case AmbientParser.NodeType.Struct:
                    {
                        ComputeTypeBodySuggestions();
                        return ConfidenceLevel.Faulty;
                    }
                
                case AmbientParser.NodeType.Method:
                case AmbientParser.NodeType.Catch:
                case AmbientParser.NodeType.GetScope:
                case AmbientParser.NodeType.AddScope:
                case AmbientParser.NodeType.RemoveScope:
                case AmbientParser.NodeType.MetaPropertyDefinitionScope:
                    {
                        ComputeFunctionBodySuggestions();
                        return ConfidenceLevel.Exact;
                    }
                case AmbientParser.NodeType.DrawStatement:
                    {
                        ComputeDrawstatementSuggestions();
                        return ConfidenceLevel.Exact;
                    }

                case AmbientParser.NodeType.Field:
                    {
                        ComputeFunctionBodySuggestions(true);
                        return ConfidenceLevel.Exact;
                    }

                case AmbientParser.NodeType.Property:
                case AmbientParser.NodeType.Indexer:
                    {
                        var p = Enumerable.Last(_context.NodePath);

                        bool hasGet = false;
                        bool hasSet = false;
                        if (p.Children != null)
                        {
                            foreach (var c in p.Children)
                            {
                                if (c.Type == AmbientParser.NodeType.GetScope) hasGet = true;
                                if (c.Type == AmbientParser.NodeType.SetScope) hasSet = true;
                            }
                        }
                        SuggestKeywords("public", "protected", "private", "internal");
                        if (!hasGet) SuggestKeywords("get");
                        if (!hasSet) SuggestKeywords("set");
                        return ConfidenceLevel.Exact;
                    }

                case AmbientParser.NodeType.SetScope:
                    {
                        ComputeFunctionBodySuggestions();
                        SuggestKeywords("value");
                        return ConfidenceLevel.Exact;
                    }

                case AmbientParser.NodeType.Operator:
                    {
                        ComputeFunctionBodySuggestions();
                        return ConfidenceLevel.Exact;
                    }

                case AmbientParser.NodeType.Constructor:
                    {
                        ComputeFunctionBodySuggestions();
                        return ConfidenceLevel.Exact;
                    }

                case AmbientParser.NodeType.Root:
                case AmbientParser.NodeType.Namespace:
                    {
                        SuggestRootItems();
                        return ConfidenceLevel.Exact;
                    }

                case AmbientParser.NodeType.InlineBlock:
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
        public static string ReadBackwardsTo(this CodeNinja.AmbientParser.IUnoCodeReader r, params TokenType[] tokenTypes)
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

        public static string ReadBackwardsTo(this CodeNinja.AmbientParser.IUnoCodeReader r, string token)
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
