using System.Collections.Generic;
using System.Linq;
using Uno.Compiler;
using Uno.Compiler.API.Domain;
using Uno.Compiler.Frontend.Analysis;

namespace Outracks.UnoDevelop.CodeNinja.AmbientParser
{
    public class ParseResult
    {
        public Node Root;
        public int CodeLengthAtParseTime;

        public ParseResult(int codeLengthAtParseTime)
        {
            CodeLengthAtParseTime = codeLengthAtParseTime;
        }
    }

    public partial class Parser
    {
        readonly Token[] _tokens;
        readonly string _code;
        int _pos;

        public static ParseResult Parse(string code)
        {
            return new Parser(Lexer.Tokenize(new SourceFile("(unknown)", code), code), code).Parse();
        }

        public static ParseResult Parse(IEnumerable<Token> tokens, string code)
        {
            return new Parser(tokens, code).Parse();
        }

        Parser(IEnumerable<Token> tokens, string code)
        {
            _tokens = tokens.ToArray();
            _code = code;
        }

        ParseResult Parse()
        {
            var root = new Node(0, int.MaxValue, NodeType.Root, "<root>");

            ParseRoot(root);

            return new ParseResult(_code.Length) { Root = root };
        }

        string LastTokenValue
        {
            get
            {
                return _tokens[_pos - 1].Value;
            }
        }

        int LastTokenOffset
        {
            get
            {
                return _tokens[_pos - 1].Offset;
            }
        }

        int NextTokenOffset
        {
            get
            {
                if (_pos < _tokens.Length) return _tokens[_pos].Offset;
                else return _code.Length - 1;
            }
        }

        void Consume()
        {
            if (NextToken != TokenType.EndOfFile) _pos++;
        }

        TokenType LastToken
        {
            get
            {
                return _tokens[_pos-1].Type;
            }
        }

        TokenType NextToken
        {
            get
            {
                if (_pos < _tokens.Length) return _tokens[_pos].Type;
                else return TokenType.EndOfFile;
            }
        }

        string NextTokenValue
        {
            get
            {
                return _tokens[_pos].Value;
            }
        }

        bool Parse(TokenType t)
        {
            if (NextToken == t)
            {
                Consume();
                return true;
            }
            return false;
        }

        bool Contextual(string keyword)
        {
            if (NextTokenValue == keyword)
            {
                Consume();
                return true;
            }
            return false;
        }

        string ParseStringTo(out TokenType hitTerminator, params TokenType[] terminators)
        {
            string s = "";
            while (NextToken != TokenType.EndOfFile)
            {
                bool found = false;
                foreach (var t in terminators) if (NextToken == t) { found = true; break; }
                if (found) break;
                s += NextTokenValue;
                Consume();
            }
            hitTerminator = NextToken;
            Consume();
            return s;
        }

        bool ParseNodeTo(Node parent, string name, int startOffset, TokenType terminator, NodeType nt, bool parseName = false)
        {
            while (true)
            {
                Consume();
                if (NextToken == terminator || NextToken == TokenType.EndOfFile)
                {
                    Consume();
                    parent.AddChild(new Node(startOffset, LastTokenOffset, nt, name));
                    return NextToken != TokenType.EndOfFile;
                }

                if(parseName)
                    name += NextTokenValue;
            }
        }

        bool ParseTo(TokenType t)
        {
            while (NextToken != t)
            {
                if (NextToken == TokenType.EndOfFile) return false;
                Consume();
            }
            return true;
        }

        bool ParseModifier()
        {
            switch (NextToken)
            {
                case TokenType.Public:
                case TokenType.Private:
                case TokenType.Protected:
                case TokenType.Virtual:
                case TokenType.Abstract:
                case TokenType.Extern:
                case TokenType.Override:
                case TokenType.Static:
                case TokenType.Const:
                    Consume();
                    return true;
                case TokenType.Identifier:
                    if (NextTokenValue == Tokens.Meta)
                    {
                        Consume();
                        return true;
                    }
                    break;
            }
            return false;
        }

        void ScopeScan(TokenType leftBrace = TokenType.LeftCurlyBrace, TokenType rightBrace = TokenType.RightCurlyBrace)
        {
            int scopeCount = 1;

            while (scopeCount > 0)
            {
                if (NextToken == leftBrace) { Consume(); scopeCount++; continue; }
                if (NextToken == rightBrace) { Consume(); scopeCount--; continue; }
                if (NextToken == TokenType.EndOfFile) break;
                Consume();
            }
        }

        bool ParseTypeAlias()
        {
            foreach (var t in TypeAliases.AllAliases)
            {
                if (NextTokenValue == t) { Consume(); return true; }
            }
            return false;
        }

        bool ParseDataType()
        {
            var old = _pos;

            if (ParseTypeAlias() || Parse(TokenType.Void))
            {
                while (Parse(TokenType.LeftSquareBrace))
                {
                    Parse(TokenType.RightSquareBrace);
                }

                return true;
            }

            while (true)
            {
                if (NextToken == TokenType.Identifier)
                {
                    Consume();
                    if (Parse(TokenType.Period)) continue;
                    else if (Parse(TokenType.LessThan))
                    {
                        while (true)
                        {
                            if (ParseDataType())
                            {
                                if (Parse(TokenType.Comma)) continue;
                                else if (Parse(TokenType.GreaterThan))
                                {
                                    while (Parse(TokenType.LeftSquareBrace))
                                    {
                                        Parse(TokenType.RightSquareBrace);
                                    }
                                    return true;

                                }
                                else
                                {
                                    _pos = old;
                                    return false;
                                }
                            }
                            else
                            {
                                _pos = old;
                                return false;
                            }
                        }
                    }
                    else
                    {
                        while (Parse(TokenType.LeftSquareBrace))
                        {
                            Parse(TokenType.RightSquareBrace);
                        }
                        return true;
                    }
                }
                else
                {
                    _pos = old;
                    return false;
                }
            }
        }

        bool ParseName(out string name)
        {
            name = "";
            if (NextToken != TokenType.Identifier) return false;

            while (true)
            {
                if (Parse(TokenType.Identifier) || Parse(TokenType.Period))
                {
                    name += LastTokenValue;
                }
                else break;
            }

            return true;
        }
    }
}
