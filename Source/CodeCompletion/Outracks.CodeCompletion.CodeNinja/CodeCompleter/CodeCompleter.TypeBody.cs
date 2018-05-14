using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Uno.Compiler;
using Uno.Compiler.Frontend.Analysis;

namespace Outracks.UnoDevelop.CodeNinja.CodeCompleter
{
    public partial class CodeCompleter
    {
        bool IsInsideMethodArgumentDeclaration()
        {
            var offs = _reader.Offset;
            while (true)
            {
                var t = _reader.ReadTokenReverse();
                switch (t)
                {
                    case TokenType.Identifier:
                    case TokenType.Whitespace:
                    case TokenType.Period:
                    case TokenType.This:
                        continue;

                    case TokenType.LeftParen:
                    {
                        _reader.Offset = offs;
                        return true;
                    }
                }
                break;
            }

            _reader.Offset = offs;
            return false;
        }

        void ComputeTypeBodySuggestions()
        {
            if (SuggestAttributesIfInsideAttributeDeclaration())
                return;

            if (IsInsideMethodArgumentDeclaration())
                SuggestExtensionMethod();

            TokenType t;
            string memberExp = FindMemberExpression(out t, true);

            if (memberExp == "")
                SuggestKeywords("public", "private", "meta", "protected", "abstract", "override", "readonly", "static", "class", "event", "struct", "enum", "delegate");

            SuggestBlock();
            SuggestTypes(memberExp, false, true);
            SuggestNodePathTypes();
        }
    }
}
