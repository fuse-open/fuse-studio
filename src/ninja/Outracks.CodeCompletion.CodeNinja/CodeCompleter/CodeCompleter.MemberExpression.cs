using System;
using System.Collections.Generic;
using Uno;
using Uno.Compiler.API.Domain;
using Uno.Compiler.Frontend.Analysis;

namespace Outracks.UnoDevelop.CodeNinja.CodeCompleter
{
    public partial class CodeCompleter
    {
        string _memberExpression = null;
        public string MemberExpression
        {
            get { return _memberExpression; }
        }

        public string FindMemberExpression(out TokenType terminatingToken, bool inTypeBody, bool skipToFirstPeriod = true)
        {
            var offset = _reader.Offset;
            int firstPeriod = skipToFirstPeriod ? -1 : offset;

            Func<string> value = () =>
            {
                _reader.ReadToken();
                if (firstPeriod != -1)
                {
                    var s = _reader.ReadText(firstPeriod - _reader.Offset).Trim();
                    _reader.Offset = offset;
                    return _memberExpression = s;
                }
                else
                {
                    _reader.Offset = offset;
                    return _memberExpression = "";
                }
            };

            int scopes = 0;
            int parens = 0;
            int triangleParens = 0;

            var tokenHistory = new List<TokenType>();

            while (true)
            {
                var t = _reader.ReadTokenReverse();
                tokenHistory.Add(t);

                switch (t)
                {
                    case TokenType.Period:
                        {
                            if (firstPeriod == -1)
                            {
                                firstPeriod = _reader.Offset;
                                continue;
                            }
                        }
                        break;

                    case TokenType.RightSquareBrace:
                    case TokenType.RightParen:
                        {
                            if (parens == 0)
                            {
                                var o2 = _reader.Offset;
                                _reader.ReadToken();
                                var t2 = _reader.ReadToken();
                                _reader.Offset = o2;
                                if (t2 != TokenType.Period && t2 != TokenType.LeftSquareBrace)
                                {
                                    terminatingToken = t;
                                    return value();
                                }
                            }
                            parens++;
                        }
                        continue;

                    case TokenType.LeftSquareBrace:
                    case TokenType.LeftParen:
                        {
                            if (parens == 0) { terminatingToken = t; return value(); }
                            --parens;
                            continue;
                        }

                    case TokenType.RightCurlyBrace:
                        {
                            if (inTypeBody || parens == 0) { terminatingToken = t; return value(); }
                            scopes++; continue;
                        }
                    case TokenType.LeftCurlyBrace:
                        {
                            if (scopes == 0) { terminatingToken = t; return value(); }
                            scopes--;
                        }
                        break;

                    case TokenType.New:
                    case TokenType.Import:
                    case TokenType.Static:
                    case TokenType.Using:
                    case TokenType.Comma:
                    case TokenType.Assign:
                    case TokenType.Plus:
                    case TokenType.Minus:
                    case TokenType.Mul:
                    case TokenType.Div:
                    case TokenType.Mod:
                    case TokenType.AddAssign:
                    case TokenType.MinusAssign:
                    case TokenType.MulAssign:
                    case TokenType.DivAssign:
                    case TokenType.ModAssign:
                    case TokenType.NotEqual:
                    case TokenType.Equal:
                    case TokenType.LessOrEqual:
                    case TokenType.GreaterOrEqual:
                    case TokenType.LogAnd:
                    case TokenType.LogOr:
                    case TokenType.ExclamationMark:
                    case TokenType.BitwiseAnd:
                    case TokenType.BitwiseOr:
                    case TokenType.BitwiseXor:
                    case TokenType.BitwiseAndAssign:
                    case TokenType.BitwiseOrAssign:
                    case TokenType.BitwiseXorAssign:
                        {
                            if (scopes == 0 && parens == 0) { terminatingToken = t; return value(); }
                        }
                        break;
                    case TokenType.GreaterThan:
                        {
                            if(triangleParens > 0 || (tokenHistory.Count > 1 && tokenHistory[tokenHistory.Count-2] == TokenType.Period))
                            {
                                ++triangleParens;
                            }
                            else
                            {
                                if (scopes == 0 && parens == 0) { terminatingToken = t; return value(); }
                            }
                        }
                        break;
                    case TokenType.LessThan:
                        {
                            if (triangleParens > 0) triangleParens--;
                            else
                            {
                                if (scopes == 0 && parens == 0) { terminatingToken = t; return value(); }
                            }
                        }
                        break;

                    case TokenType.Colon:
                    case TokenType.Semicolon:
                        {
                            if (scopes == 0) { terminatingToken = t; return value(); }
                        }
                        break;

                    case TokenType.EndOfFile:
                        terminatingToken = t;
                        return "";

                    case TokenType.This: break;

                    case TokenType.Identifier:
                        {
                            var w = _reader.PeekTokenReverse();

                            if (scopes == 0 && parens == 0 &&  w == TokenType.Whitespace)
                            {
                                _reader.ReadTokenReverse();
                                w = _reader.PeekTokenReverse();
                                if (scopes == 0 && parens == 0 && (w == TokenType.Identifier || TypeAliases.HasAlias(w.ToLiteral())))
                                {
                                    terminatingToken = t;
                                    return value();
                                }
                                _reader.ReadToken();

                                var o2 = _reader.Offset;
                                t = _reader.ReadToken();
                                var t1 = _reader.PeekToken();
                                _reader.Offset = o2;

                                if (t1 == TokenType.LeftParen)
                                {
                                    _reader.ReadTokenReverse();
                                    terminatingToken = t;
                                    return value();
                                }
                            }
                        }
                        break;

                    default:
                        {
                            if (Tokens.IsReserved(t.ToLiteral()))
                            {
                                if (scopes == 0 && parens == 0) { terminatingToken = t; return value(); }
                            }
                        }
                        break;
                }
            }
        }
    }
}
