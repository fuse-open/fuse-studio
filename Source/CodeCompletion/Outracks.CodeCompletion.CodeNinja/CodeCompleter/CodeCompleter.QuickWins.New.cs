using Outracks.CodeCompletion;
using Uno.Compiler.API.Domain;
using Uno.Compiler.API.Domain.IL;
using Uno.Compiler.Core.Syntax.Compilers;
using Uno.Compiler.Core.Syntax.Binding;
using Uno.Compiler.Frontend.Analysis;

namespace Outracks.UnoDevelop.CodeNinja.CodeCompleter
{
    public partial class CodeCompleter
    {
        bool TrySuggestVariableDeclarationType()
        {
            int start = _reader.Offset;

            bool foundIdentifier = false;
            string variableName = "";
            int variableNameStart = 0;

            // Check to see if this is a variable declaration
            _reader.ReadTokenReverse();
            var currentToken = _reader.ReadTokenReverse();
            if (currentToken == TokenType.New) currentToken = _reader.ReadTokenReverse();
            while (currentToken == TokenType.Whitespace) currentToken = _reader.ReadTokenReverse();
            if (currentToken == TokenType.Assign)
            { 
                // Looks like it; let's try to get the type
                currentToken = _reader.ReadTokenReverse();
                while (currentToken == TokenType.Whitespace) { variableNameStart = _reader.Offset; currentToken = _reader.ReadTokenReverse(); }
                if (currentToken == TokenType.Identifier)
                {
                    variableName = _reader.PeekText(variableNameStart - _reader.Offset);
                    int endOffset;
                    do
                    {
                        endOffset = _reader.Offset;
                        currentToken = _reader.ReadTokenReverse();
                    } while (currentToken == TokenType.Whitespace);


                    if (currentToken == TokenType.Identifier || currentToken == TokenType.GreaterThan)
                    {
                        // If this isn't a type, then fuck my uncle and call me.. nevermind
                        bool done = false;
                        int paramLevel = 0;
                        while (!done)
                        {
                            switch (currentToken)
                            {
                                case TokenType.Identifier:
                                    currentToken = _reader.ReadTokenReverse();
                                    break;

                                case TokenType.GreaterThan:
                                    paramLevel++;
                                    currentToken = _reader.ReadTokenReverse();
                                    break;

                                case TokenType.Shr:
                                    paramLevel += 2;
                                    currentToken = _reader.ReadTokenReverse();
                                    break;

                                case TokenType.LessThan:
                                    paramLevel--;
                                    currentToken = _reader.ReadTokenReverse();
                                    break;

                                case TokenType.Shl:
                                    paramLevel -= 2;
                                    currentToken = _reader.ReadTokenReverse();
                                    break;

                                case TokenType.Period:
                                    currentToken = _reader.ReadTokenReverse();
                                    break;

                                case TokenType.Comma:
                                case TokenType.Whitespace:
                                    if (paramLevel <= 0)
                                    {
                                        done = true;
                                    }
                                    else
                                    {
                                        currentToken = _reader.ReadTokenReverse();
                                    }
                                    break;
                                
                                default:
                                    done = true;
                                    break;
                            }
                        }
                        var type = _reader.ReadText(endOffset - _reader.Offset).Trim();
                        var functionCompiler = new FunctionCompiler(_context.Compiler, _context.TypeOrNamespace);
                        var pe = ResolveStringInFunctionContext(_methodNode, type, _source, functionCompiler) as PartialType;
                        if (pe != null)
                        {
                            Suggest(SuggestItemType.Class, pe.Type, type, null, null, null, null, null, null, null, SuggestItemPriority.High);
                            foundIdentifier = true;
                        }
                    }
                }
            }

            // Try to find variable declaration for variable.
            if(!foundIdentifier)
                foundIdentifier = TrySuggestVariableTypes(variableName);

            _reader.Offset = start;
            return foundIdentifier;
        }

        bool TrySuggestVariableTypes(string varName)
        {
            if (varName == "") return false;

            var functionCompiler = new FunctionCompiler(_context.Compiler, _context.TypeOrNamespace);
            DataType dt = functionCompiler.Function.DeclaringType;
            if (dt != null)
            {
                foreach (var member in dt.Fields)
                {
                    if (!CompatibleAccessor(AccessorLevel.Private, member)) continue;
                    if (member.Modifiers.HasFlag(Modifiers.Generated)) continue;
                    if ((_context.TypeOrNamespace is DataType) &&
                        !IsMemberAccessible(member, (_context.TypeOrNamespace as DataType))) continue;

                    if (member.Name == varName)
                    {
                        string type = member.ReturnType.FullName;
                        var typeExps = type.Split(".");
                        var currentType = "";

                        for (var i = typeExps.Length - 1; i >= 0; --i)
                        {
                            currentType = currentType == "" ? typeExps[i] : typeExps[i] + "." + currentType;
                            var pe =
                                ResolveStringInFunctionContext(_methodNode, currentType, _source, functionCompiler) as
                                PartialType;
                            if (pe != null) break;
                        }

                        Suggest(SuggestItemType.Class, member.ReturnType, currentType, null, null, null, null, null, null, null, SuggestItemPriority.High);
                        return true;
                    }
                }
            }

            return false;
        }
    }
}
