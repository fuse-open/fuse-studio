using System.Linq;
using Uno.Compiler;
using Uno.Compiler.API.Domain;
using Uno.Compiler.API.Domain.IL;
using Uno.Compiler.Frontend.Analysis;

namespace Outracks.UnoDevelop.CodeNinja.CodeCompleter
{
    public partial class CodeCompleter
    {
        ConfidenceLevel ComputeQuickWins()
        {
            // Completion on strings in import/apply
            if (SuggestImportApplyFiles()) return ConfidenceLevel.Exact;

            // early-out of strings and comments
            switch (_reader.PeekToken())
            {
                case TokenType.String:
                case TokenType.SingleLineComment:
                case TokenType.MultiLineComment:
                    return ConfidenceLevel.Exact;
            }

            // Scan backwards to find first significant token
            int start = _reader.Offset;
            bool scan = true;
            bool readmultiple = false;
            _context.expectsConstructor = false;
            while (scan)
            {
                var t = _reader.ReadTokenReverse();

                switch (t)
                {
                    case TokenType.New:
                         {
                            if (start - _reader.Offset >= 4 && char.IsWhiteSpace(_reader.PeekText(4)[3]))
                            {
                                _reader.Offset = start;
                                var qualifier = _reader.ReadBackwardsTo(TokenType.New).Trim();

                                if (qualifier != "")
                                {
                                    //There is a qualifier and a new token, this means we're good to complete to constructors
                                    _context.expectsConstructor = true;
                                }
                                
                                if (!TrySuggestVariableDeclarationType())
                                    return ConfidenceLevel.Failed;

                                if (qualifier.Contains('.'))
                                {
                                    qualifier = qualifier.Substring(0, qualifier.LastIndexOf('.'));
                                    SuggestTypes(qualifier, false, false);
                                }
                                else
                                {
                                    SuggestTypes("", false, false);
                                }
                            }
                            return ConfidenceLevel.Exact;
                        }

                    case TokenType.Override:
                        SuggestOverrides();
                        return ConfidenceLevel.Faulty;

                    case TokenType.Import:
                        _reader.Offset = start;
                        SuggestImports();
                        return ConfidenceLevel.Exact;
                        
                    case TokenType.For:
                    case TokenType.Foreach:
                    case TokenType.Switch:
                    case TokenType.If:
                    case TokenType.While:
                        _reader.Offset = start;
                        return readmultiple ? ConfidenceLevel.Failed : ConfidenceLevel.Exact;


                    case TokenType.StringLiteral:
                    case TokenType.FloatLiteral:
                    case TokenType.DoubleLiteral:
                    case TokenType.HexadecimalLiteral:
                    case TokenType.DecimalLiteral:
                        _reader.Offset = start;
                        if (readmultiple) return ConfidenceLevel.Failed;
                        else return ConfidenceLevel.Exact;


                    case TokenType.Using:
                        {
                            _reader.Offset = start;
                            var qualifier = _reader.ReadBackwardsTo(TokenType.Using);
                            if (!qualifier.Contains(','))
                            {
                                _reader.Offset = start;
                                if (FindFirstReverse(TokenType.Using, TokenType.Static) != TokenType.Static)
                                {
                                    SuggestUsingOrApply(qualifier.Trim(), true, false);
                                }
                                else
                                {
                                    _reader.Offset = start;
                                    SuggestUsingOrApply(_reader.ReadBackwardsTo(TokenType.Static).Trim(), true);
                                    foreach (var u in _context.Usings)
                                    {
                                        var ns = _context.Compiler.ILFactory.GetEntity(_context.Root.Source, u, _context.Root);
                                        if (ns is Namespace) Suggest(
                                            from dt in (ns as Namespace).Types
                                            where dt.IsStatic
                                            select dt);
                                    }
                                }
                                return ConfidenceLevel.Exact;
                            }
                            return ConfidenceLevel.Failed;
                        }

                    case TokenType.Namespace:
                    case TokenType.Class:
                    case TokenType.Struct:
                    case TokenType.Enum:
                        // Entity name - no suggestions
                        return ConfidenceLevel.Exact;

                    case TokenType.Colon:
                        {
                            if (TokenType.Class == FindFirstReverse(TokenType.Class, TokenType.Semicolon, TokenType.LeftCurlyBrace, TokenType.RightCurlyBrace))
                            {
                                // Base type, suggest all types
                                _reader.Offset = start;
                                var temp = _reader.ReadBackwardsTo(TokenType.Colon, TokenType.Comma);
                                var qualifier = temp.Trim();

                                var memberExp = "";
                                if (qualifier.LastIndexOf('.') != -1)
                                {
                                    var x = qualifier.LastIndexOf('.');
                                    memberExp = qualifier.Substring(0, x);
                                }

                                _reader.ReadTextReverse(temp.Length);
                                _reader.ReadTextReverse(_reader.ReadBackwardsTo(TokenType.Identifier).Length);
                                var className = _reader.ReadBackwardsTo(TokenType.Whitespace).Trim();
                                SuggestTypes(memberExp, true, false, SuggestTypesMode.Everything, true);
                                _suggestions.RemoveAll(x => x.Text == className);
                                return ConfidenceLevel.Exact;
                            }
                        }
                        break;



                    case TokenType.Semicolon:
                    case TokenType.LeftCurlyBrace:
                    case TokenType.RightCurlyBrace:
                    case TokenType.LeftSquareBrace:
                    case TokenType.LeftParen:
                    case TokenType.RightParen:
                        // More complex case, fall through to AST-based analysis
                        _reader.Offset = start;
                        scan = false;
                        return ConfidenceLevel.Failed;

                    case TokenType.Null:
                        return ConfidenceLevel.Exact;
                }

                readmultiple = true;
            }

            return ConfidenceLevel.Failed;

        }
    }
}
