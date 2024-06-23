using System.Linq;
using Uno.Compiler.Frontend.Analysis;

namespace Outracks.UnoDevelop.CodeNinja.AmbientParser
{
    public partial class Parser
    {
        void ParseClassOrBlockBody(Node c)
        {
            while (NextToken != TokenType.EndOfFile)
            {
                int start = NextTokenOffset;

                if (Parse(TokenType.RightCurlyBrace)) break;

                if (Contextual(Tokens.Apply))
                {
                    ParseNodeTo(c, NextToken == TokenType.Identifier ? NextTokenValue : "apply", LastTokenOffset, TokenType.Semicolon, NodeType.Apply, true);
                    continue;
                }

                if (ParseModifier()) continue;

                if (ParseTypeOrBlockDeclaration(c))
                {
                    continue;
                }

                if (Contextual(Tokens.Meta))
                {
                    Parse(TokenType.Identifier);
                    Parse(TokenType.LeftCurlyBrace);
                    ParseClassOrBlockBody(c);
                    continue;
                }

                // Operators (operator <return_type> (...))
                if (Parse(TokenType.Operator))
                {
                    ParseDataType();
                    ParseFunctionWithParameterList(c, NodeType.Operator, "", start);
                    continue;
                }

                int dataTypeStart = _pos;
                if (ParseDataType())
                {
                    int dataTypeEnd = _pos;

                    // Indexers
                    if (Parse(TokenType.This))
                    {
                        ParseParameterList(TokenType.LeftSquareBrace, TokenType.RightSquareBrace);

                        if (Parse(TokenType.LeftCurlyBrace))
                        {
                            var p = new Node(start, 0, NodeType.Indexer, "Item");
                            ParseGetSetScope(p);
                            p.EndOffset = LastTokenOffset;
                            c.AddChild(p);
                            continue;
                        }
                    }

                    // Operators (<return_type> operator <op>(...))
                    if (Parse(TokenType.Operator))
                    {
                        Consume(); // consume operator name
                        ParseFunctionWithParameterList(c, NodeType.Operator, LastTokenValue, start);
                        continue;
                    }

                    // Constructors
                    if (NextToken == TokenType.LeftParen)
                    {
                        ParseFunctionWithParameterList(c, NodeType.Constructor, LastTokenValue, start);
                        continue;
                    }

                    string name;
                    // Fields, methods, properties or meta properties
                    if (ParseName(out name))
                    {
                        // Fields without initializer
                        if (Parse(TokenType.Semicolon))
                        {
                            c.AddChild(new Node(start, LastTokenOffset, NodeType.Field, name));
                            continue;
                        }

                        // Fields with initializer
                        if (Parse(TokenType.Assign))
                        {
                            ParseNodeTo(c, name, start, TokenType.Semicolon, NodeType.Field);
                            continue;
                        }

                        // Properties
                        if (Parse(TokenType.LeftCurlyBrace))
                        {
                            var p = new Node(start, 0, NodeType.Property, name);
                            ParseGetSetScope(p);
                            p.EndOffset = LastTokenOffset;
                            c.AddChild(p);
                            continue;
                        }

                        // Generic method arguments
                        if (Parse(TokenType.LessThan))
                        {
                            while (true)
                            {
                                if (Parse(TokenType.GreaterThan)) break;
                                if (Parse(TokenType.EndOfFile)) return;
                                Consume();
                            }
                        }

                        // Methods
                        if (ParseFunctionWithParameterList(c, NodeType.Method, name, start))
                        {
                            continue;
                        }

                        // Fall through to handle meta properties with types
                    }

                    ParseMetaProperty(start, c, dataTypeStart, dataTypeEnd);
                }
                else
                {
                    // Else ignore unrecognized token
                    Consume();
                }
            }
        }

        void ParseMetaProperty(int start, Node c, int dataTypeStart, int dataTypeEnd)
        {
            // Meta properties
            var metaName = LastTokenValue;
            if (Parse(TokenType.Colon))
            {
                int scopeCount = 0;
                var meta = new Node(start, 0, NodeType.MetaProperty, metaName);
                c.AddChild(meta);

                int defCount = 1;

                var def = new Node(LastTokenOffset, 0, NodeType.MetaPropertyDefinition, "Definition" + defCount);
                meta.AddChild(def);

                while (NextToken != TokenType.EndOfFile)
                {
                    if (scopeCount < 1)
                    {
                        if (NextToken == TokenType.Semicolon || NextToken == TokenType.Comma)
                        {
                            def.EndOffset = LastTokenOffset;
                            if (def.Children != null) def.Children.First().EndOffset = LastTokenOffset;

                            if (Parse(TokenType.Comma))
                            {
                                defCount++;
                                def = new Node(LastTokenOffset, 0, NodeType.MetaPropertyDefinition, "Definition" + defCount);
                                meta.AddChild(def);
                                continue;
                            }
                            else if (Parse(TokenType.Semicolon))
                            {
                                break;
                            }
                        }
                        if (Parse(TokenType.LeftCurlyBrace))
                        {
                            scopeCount++;
                            def.AddChild(new Node(LastTokenOffset, _code.Length - 1, NodeType.MetaPropertyDefinitionScope, "DefinitionScope"));
                            continue;
                        }
                    }
                    if (Parse(TokenType.LeftParen)) scopeCount++;
                    else if (Parse(TokenType.RightParen)) scopeCount--;
                    else if (Parse(TokenType.LeftCurlyBrace)) scopeCount++;
                    else if (Parse(TokenType.RightCurlyBrace)) scopeCount--;
                    else Consume();

                    if (scopeCount < 0) break;
                }
                def.EndOffset = LastTokenOffset;
                meta.EndOffset = LastTokenOffset;
                if (def.Children != null) { if (def.Children.First().EndOffset == _code.Length - 1) def.Children.First().EndOffset = LastTokenOffset; }

            }
        }

        bool ParseParameterList(TokenType leftParen = TokenType.LeftParen, TokenType rightParen = TokenType.RightParen)
        {
            if (!Parse(leftParen)) return false;

            while (NextToken != TokenType.EndOfFile)
            {
                Parse(TokenType.Out);
                Parse(TokenType.Ref);

                ParseDataType();
                Parse(TokenType.Identifier);

                if (Parse(TokenType.Comma)) continue;
                if (NextToken == TokenType.LeftParen) ParseParameterList(leftParen, rightParen);
                if (Parse(rightParen)) return true;
                Consume();
                //break;
            }
            return true;
        }

        void ParseDrawStatement(Node func)
        {
            int blockListStart = NextTokenOffset;

            var ds = new Node(LastTokenOffset, -1, NodeType.DrawStatement, "DrawStatement");
            func.AddChild(ds);

            while (NextToken != TokenType.EndOfFile)
            {
                if (Parse(TokenType.Semicolon))
                {
                    ds.DrawStatementBlockList = _code.Substring(blockListStart, LastTokenOffset - blockListStart);
                    ds.EndOffset = LastTokenOffset;
                    return;
                }

                if (Parse(TokenType.LeftCurlyBrace))
                {
                    ds.DrawStatementBlockList = _code.Substring(blockListStart, LastTokenOffset - blockListStart);
                    var ib = new Node(LastTokenOffset, -1, NodeType.InlineBlock, "InlineBlock");
                    ParseClassOrBlockBody(ib);
                    ib.EndOffset = LastTokenOffset;
                    ds.EndOffset = LastTokenOffset;
                    ds.AddChild(ib);
                    return;
                }

                Consume();
            }
        }

        bool ParseFunctionBody(Node func)
        {
            if (!Parse(TokenType.LeftCurlyBrace)) return false;

            int scopeCount = 1;
            while (scopeCount != 0 && NextToken != TokenType.EndOfFile)
            {
                if (Contextual(Tokens.Draw))
                {
                    ParseDrawStatement(func);
                    continue;
                }

                if (Parse(TokenType.Catch))
                {
                    Node n = new Node(-1, -1, NodeType.Catch, "");
                    func.AddChild(n);

                    var start = LastTokenOffset;
                    n.StartOffset = start;
                    n.EndOffset = int.MaxValue;

                    if (!ParseTo(TokenType.LeftCurlyBrace)) return true;
                    Consume();

                    ScopeScan();

                    n.EndOffset = LastTokenOffset;

                    continue;
                }

                if (Parse(TokenType.LeftCurlyBrace)) { scopeCount++; continue; }
                else if (Parse(TokenType.RightCurlyBrace)) { scopeCount--; continue; }
                Consume();
            }

            return true;
        }

        void ParseGetSetScope(Node p)
        {
            while (NextToken != TokenType.EndOfFile)
            {
                while (ParseModifier()) { }

                if (NextToken == TokenType.Identifier && NextTokenValue == "get")
                {
                    Consume();

                    if (Parse(TokenType.Semicolon)) continue;

                    var getStart = NextTokenOffset;
                    var func = new Node(getStart, -1, NodeType.GetScope, "GetScope");
                    if (ParseFunctionBody(func))
                    {
                        func.EndOffset = LastTokenOffset;
                        p.AddChild(func);
                    }
                }
                else if (NextToken == TokenType.Identifier && NextTokenValue == "set")
                {
                    Consume();

                    if (Parse(TokenType.Semicolon)) continue;

                    var getStart = NextTokenOffset;
                    var func = new Node(getStart, -1, NodeType.SetScope, "SetScope");
                    if (ParseFunctionBody(func))
                    {
                        func.EndOffset = LastTokenOffset;
                        p.AddChild(func);
                    }
                }
                else if (Parse(TokenType.RightCurlyBrace)) break;
                else Consume();
            }
        }

        bool ParseFunctionWithParameterList(Node c, NodeType type, string name, int start)
        {
            if (!ParseParameterList()) return false;

            // Bodyless function
            if (Parse(TokenType.Semicolon))
            {
                c.AddChild(new Node(start, LastTokenOffset, type, name));
                return true;
            }

            if (Parse(TokenType.Colon))
            {
                if (Parse(TokenType.Base) && Parse(TokenType.LeftParen))
                {
                    ScopeScan(TokenType.LeftParen, TokenType.RightParen);
                }
                else
                {
                    while (true)
                    {
                        if (NextToken == TokenType.LeftCurlyBrace) break;
                        if (NextToken == TokenType.EndOfFile) return true;
                        Consume();
                    }
                }
            }

            // Function body
            var func = new Node(start, LastTokenOffset, type, name);
            if (ParseFunctionBody(func))
            {
                func.EndOffset = LastTokenOffset;
                c.AddChild(func);
                return true;
            }

            return false;
        }

        void ParseEventBody(Node p)
        {
            while (NextToken != TokenType.EndOfFile)
            {
                if (NextTokenValue == Tokens.Add)
                {
                    Consume();

                    var getStart = NextTokenOffset;
                    var func = new Node(getStart, -1, NodeType.AddScope, "AddScope");
                    if (ParseFunctionBody(func))
                    {
                        func.EndOffset = LastTokenOffset;
                        p.AddChild(func);
                    }
                }
                else if (NextTokenValue == Tokens.Remove)
                {
                    Consume();

                    var getStart = NextTokenOffset;
                    var func = new Node(getStart, -1, NodeType.RemoveScope, "RemoveScope");
                    if (ParseFunctionBody(func))
                    {
                        func.EndOffset = LastTokenOffset;
                        p.AddChild(func);
                    }
                }
                else if (Parse(TokenType.Semicolon)) break;
                else if (Parse(TokenType.RightCurlyBrace)) break;
                else Consume();
            }
        }
    }
}
