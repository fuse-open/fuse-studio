using System.Collections.Generic;
using Uno.Compiler.Frontend.Analysis;

namespace Outracks.UnoDevelop.CodeNinja.AmbientParser
{
    public partial class Parser
    {
        void ParseRoot(Node parent)
        {
            while (NextToken != TokenType.EndOfFile)
            {
                if (Parse(TokenType.RightCurlyBrace)) break;

                if (Parse(TokenType.Using))
                {
                    var usingType = Parse(TokenType.Static) ? NodeType.UsingStatic : NodeType.Using;
                    TokenType hit;
                    int startOffset = LastTokenOffset;
                    var usingName = NextToken == TokenType.Identifier ? ParseStringTo(out hit, TokenType.Semicolon) : "using";
                    parent.AddChild(new Node(LastTokenOffset, startOffset, usingType, usingName));
                    continue;
                }

                if (Parse(TokenType.Namespace))
                {
                    TokenType hit;
                    var name = ParseStringTo(out hit, TokenType.LeftCurlyBrace);
                    if (hit == TokenType.EndOfFile) return;

                    List<Node> namespaces = null;
                    Node ns = parent;

                    namespaces = new List<Node>();
                    var parts = name.Split('.');

                    for (int i = 0; i < parts.Length; i++)
                    {
                        var nc = new Node(LastTokenOffset, -1, NodeType.Namespace, parts[i]);
                        if (ns != null) ns.AddChild(nc);
                        namespaces.Add(nc);
                        ns = nc;
                    }

                    ParseRoot(ns);

                    if (namespaces != null)
                        foreach (var nns in namespaces) nns.EndOffset = NextTokenOffset;
                    else
                        ns.EndOffset = NextTokenOffset;

                    continue;
                }

                if (ParseTypeOrBlockDeclaration(parent))
                {
                    continue;
                }

                // Skip unknown tokens
                Consume();
            }
        }

        void SkipAttributes()
        {
            while (Parse(TokenType.LeftSquareBrace))
            {
                int c = 1;
                while (c > 0)
                {
                    if (Parse(TokenType.LeftSquareBrace)
                        || Parse(TokenType.LeftParen)
                        || Parse(TokenType.LeftCurlyBrace))
                    {
                        c++;
                        continue;
                    }
                    else if (Parse(TokenType.RightSquareBrace)
                        || Parse(TokenType.RightParen)
                        || Parse(TokenType.RightCurlyBrace))
                    {
                        c--;
                        continue;
                    }
                    else if (NextToken == TokenType.EndOfFile) break;
                    else Consume();
                }
            }
        }

        bool ParseTypeOrBlockDeclaration(Node parent)
        {
            SkipAttributes();

            while (ParseModifier()) { }

            if (Contextual(Tokens.Block))
            {
                TokenType terminator;
                var c = new Node(LastTokenOffset, -1, NodeType.Block, ParseStringTo(out terminator, TokenType.Colon, TokenType.LeftCurlyBrace));
                if (terminator == TokenType.EndOfFile) return false;

                ParseClassOrBlockBody(c);
                c.EndOffset = LastTokenOffset;

                parent.AddChild(c);
                return true;
            }

            if (Parse(TokenType.Class))
            {
                TokenType terminator;
                var c = new Node(LastTokenOffset, -1, NodeType.Class, ParseStringTo(out terminator, TokenType.Colon, TokenType.LeftCurlyBrace, TokenType.LessThan));

                if (terminator == TokenType.LessThan)
                {
                    while (true)
                    {
                        if (Parse(TokenType.Colon)) { terminator = TokenType.Colon; break; }
                        if (Parse(TokenType.LeftCurlyBrace)) { terminator = TokenType.LeftCurlyBrace; break; }
                        if (Parse(TokenType.EndOfFile)) { break; }
                        Consume();
                    }
                }

                TokenType hit;
                if (terminator == TokenType.Colon)
                {
                    c.BaseClassName = ParseStringTo(out hit, TokenType.LeftCurlyBrace, TokenType.LessThan);

                    if (hit == TokenType.EndOfFile) return false;
                }

                ParseClassOrBlockBody(c);

                c.EndOffset = LastTokenOffset;

                parent.AddChild(c);
                return true;
            }

            if (Parse(TokenType.Interface))
            {
                TokenType terminator;
                var c = new Node(LastTokenOffset, -1, NodeType.Interface, ParseStringTo(out terminator, TokenType.Colon, TokenType.LeftCurlyBrace, TokenType.LessThan));

                if (terminator == TokenType.LessThan)
                {
                    while (true)
                    {
                        if (Parse(TokenType.Colon)) { terminator = TokenType.Colon; break; }
                        if (Parse(TokenType.LeftCurlyBrace)) { terminator = TokenType.LeftCurlyBrace; break; }
                        if (Parse(TokenType.EndOfFile)) { break; }
                        Consume();
                    }
                }

                TokenType hit;
                if (terminator == TokenType.Colon)
                {
                    c.BaseClassName = ParseStringTo(out hit, TokenType.LeftCurlyBrace, TokenType.LessThan);

                    if (hit == TokenType.EndOfFile) return false;
                }

                ParseClassOrBlockBody(c);

                c.EndOffset = LastTokenOffset;

                parent.AddChild(c);
                return true;
            }

            if (Parse(TokenType.Struct))
            {
                TokenType hit;
                var c = new Node(LastTokenOffset, -1, NodeType.Struct, ParseStringTo(out hit, TokenType.LeftCurlyBrace));
                if (hit == TokenType.EndOfFile) return false;

                ParseClassOrBlockBody(c);

                c.EndOffset = LastTokenOffset;

                parent.AddChild(c);
                return true;
            }

            if (Parse(TokenType.Enum))
            {
                TokenType hit;
                var c = new Node(LastTokenOffset, -1, NodeType.Enum, ParseStringTo(out hit, TokenType.LeftCurlyBrace));
                if (hit == TokenType.EndOfFile) return false;

                TokenType terminator;
                while (true)
                {
                    SkipAttributes();
                    ParseStringTo(out terminator, TokenType.Comma, TokenType.RightCurlyBrace);
                    if (terminator == TokenType.RightCurlyBrace || terminator == TokenType.EndOfFile) break;
                }

                c.EndOffset = LastTokenOffset;

                parent.AddChild(c);
                return true;
            }

            if (Parse(TokenType.Delegate))
            {
                ParseNodeTo(parent, "delegate", LastTokenOffset, TokenType.Semicolon, NodeType.Delegate);
                return true;
            }

            if (Parse(TokenType.Event))
            {
                ParseDataType();

                TokenType hit;
                var c = new Node(LastTokenOffset, -1, NodeType.Event, ParseStringTo(out hit, TokenType.LeftCurlyBrace, TokenType.Semicolon));
                if (hit == TokenType.EndOfFile) return false;

                if(hit != TokenType.Semicolon)
                    ParseEventBody(c);

                c.EndOffset = LastTokenOffset;

                parent.AddChild(c);
                return true;
            }

            return false;
        }
    }
}
