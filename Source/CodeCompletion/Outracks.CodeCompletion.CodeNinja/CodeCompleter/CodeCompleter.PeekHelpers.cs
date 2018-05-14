using System.Linq;
using Uno.Compiler.Frontend.Analysis;

namespace Outracks.UnoDevelop.CodeNinja.CodeCompleter
{
    public partial class CodeCompleter
    {
        TokenType FindFirstReverse(params TokenType[] tokens)
        {
            var start = _reader.Offset;

            while (true)
            {
                var t = _reader.ReadTokenReverse();
                if (tokens.Contains(t)) 
                {
                    _reader.Offset = start;
                    return t;
                }
                if (_reader.Offset == 0)
                {
                    _reader.Offset = start;
                    return 0;
                }
            }
        }

        bool IsBeforeScopeStart(AmbientParser.Node n)
        {
            if (n == null) return false;

            var offset = _reader.Offset;

            _reader.Offset = n.StartOffset;
            while (true)
            {
                var t = _reader.ReadToken();
                if (t == TokenType.EndOfFile || t == TokenType.LeftCurlyBrace) break;
            }

            var res = _reader.Offset > offset;
            _reader.Offset = offset;
            return res;
        }

        int GetScopeStart(int startOffset)
        {
            var offset = _reader.Offset;

            _reader.Offset = startOffset;
            while (true)
            {
                var t = _reader.ReadToken();
                if (t == (TokenType)(-1) || t == TokenType.EndOfFile || t == TokenType.LeftCurlyBrace)
                {
                    _reader.ReadTokenReverse();
                    break;
                }
            }

            var res = _reader.Offset;
            _reader.Offset = offset;
            return res;
        }

        bool InNameDeclarationOrOperatorPosition()
        {
            int offset = _reader.Offset;
            
            if (offset < 1) return false;

            char c = _reader.ReadTextReverse(1)[0];

            TokenType t;

            if (char.IsLetterOrDigit(c) || c == '_')
            {
                 t = _reader.ReadTokenReverse();

                if (t != TokenType.Whitespace)
                {
                    _reader.Offset = offset;
                    return false;
                }

            }
            else if (char.IsWhiteSpace(c))
            {
                t = _reader.PeekToken();
                if (t != TokenType.Whitespace)
                {
                    _reader.Offset = offset;
                    return false;
                }
            }
            else
            {
                _reader.Offset = offset;
                return false;
            }

            t = _reader.ReadTokenReverse();
            _reader.Offset = offset;

            if (t == TokenType.Identifier || t == TokenType.GreaterThan || t == TokenType.Void)
            {
                return true;
            }

            // Checks if we are working with a nested generic identifier.
            if (t == TokenType.Shr)
            {
                int lessThanCount = 0;
                int identifierCount = 0;
                while(lessThanCount >= identifierCount - 1)
                {
                    t = _reader.ReadTokenReverse();
                    if(t == TokenType.LessThan)
                    {
                        ++lessThanCount;
                    }
                    else if (t == TokenType.Identifier || t == TokenType.GreaterThan || t == TokenType.Void)
                    {
                        ++identifierCount;
                    }
                }

                return lessThanCount == 2;
            }

            return false;
        }
    }
}
