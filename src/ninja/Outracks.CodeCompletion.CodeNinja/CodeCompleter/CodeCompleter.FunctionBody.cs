using System;
using System.Linq;
using Outracks.UnoDevelop.CodeNinja.AmbientParser;
using Uno;
using Uno.Compiler.API.Domain.AST.Statements;
using Uno.Compiler.API.Domain.IL;
using Uno.Compiler.Core.Syntax.Binding;
using Uno.Compiler.Core.Syntax.Compilers;
using Uno.Compiler.Frontend.Analysis;
using Parser = Uno.Compiler.Frontend.Analysis.Parser;

namespace Outracks.UnoDevelop.CodeNinja.CodeCompleter
{
    public partial class CodeCompleter
    {
        public FunctionCompiler CreateFunctionCompiler(Node methodNode)
        {
            int startOffset;
           var functionBody = ParseFunctionBody(methodNode, out startOffset);

            // Find the function we are inside
            Function func = null;
            int funcDist = int.MaxValue;
            if (methodNode != null && _context.TypeOrNamespace is DataType)
            {
                foreach (var f in (_context.TypeOrNamespace as DataType).EnumerateFunctions()
					.Where(m => m.Source.FullPath == _source.FullPath))
                {
                    int dist = Math.Abs(f.Source.Offset - methodNode.StartOffset);
                    if (dist < funcDist)
                    {
                        func = f;
                        funcDist = dist;
                    }
                }
            }

            // Construct a function compiler for the context
            FunctionCompiler funcCompiler;
            if (func != null)
            {
                funcCompiler = new FunctionCompiler(_compiler, func, _compiler.TypeBuilder.Parameterize(func.DeclaringType), functionBody);
                try
                {
                    funcCompiler.Compile();
                }
                catch (Exception) { }
            }
            else
            {
                funcCompiler = new FunctionCompiler(_compiler, _context.TypeOrNamespace);
            }

            return funcCompiler;
        }

        public AstScope ParseFunctionBody(Node methodNode, out int startOffset)
        {
            string funcCode = "{";

            var funcStart = 0;
            if (methodNode != null)
            {
                funcStart = GetScopeStart(methodNode.StartOffset);
                if (funcStart <= _reader.Offset)
                {
                    funcCode = _reader.PeekTextReverse(_reader.Offset - funcStart);
                }
            }

            startOffset = funcStart;

            return Parser.ParseStatement(_compiler.Log, _source, funcCode) as AstScope;
        }

        public PartialExpression ResolveStringInFunctionContext(Node methodNode, string memberExp, Source src, FunctionCompiler fc = null)
        {
            var funcCompiler = fc ?? CreateFunctionCompiler(methodNode);

            // Parse the member expression
            var dte = Parser.ParseExpression(_compiler.Log, src, memberExp, fc != null && fc.MetaProperty != null ? ParseContext.MetaProperty : ParseContext.Default);

            if (!dte.IsInvalid)
            {
                // Resolve the member expresion
                return funcCompiler.ResolveExpression(dte, null);
            }

            return null;
        }

        void ComputeFunctionBodySuggestions(bool isFieldInitializer = false)
        {
            _exceptionTypesOnly = InCatchBlockHeader();

            TokenType t;
            string memberExp = FindMemberExpression(out t, false);

            if (IsBeforeScopeStart(_methodNode))
            {
                var startOffs = _reader.Offset;
                bool done = false;
                bool isInArgumentList = true;
                while (!done && _reader.Offset > 0)
                {
                    switch (_reader.ReadTokenReverse())
                    {
                        case TokenType.Colon:
                            isInArgumentList = false;
                            done = true;
                            break;

                        case TokenType.LeftParen:
                        case TokenType.RightParen:
                            done = true;
                            break;

						case TokenType.LeftSquareBrace:
		                    _reader.Offset++; //Step ahead to allow the attribute suggester to re-read the left square brace
		                    SuggestAttributesIfInsideAttributeDeclaration();
			                return; //If there's no attribute the expression is invalid anyway
                    }
                }

                if (isInArgumentList)
                {
                    // In argument list
                    if (memberExp == "" && !_exceptionTypesOnly)
                        SuggestKeywords("out", "ref");

                    _reader.Offset = startOffs;
                    SuggestExtensionMethod();
                    SuggestTypes(memberExp, isFieldInitializer, false);
                }
            }
            else
            {
                if (memberExp == "")
                {
                    SuggestKeywords("new", "true", "false", "null", "ref", "out", "import");

                    if (!isFieldInitializer)
                        SuggestKeywords("debug_log", "var", "for", "foreach", "case", "default", "break", "if", "while", "do", "switch", "try", "else", "catch", "throw", "draw", "return", "assert");
                }

                var last = Enumerable.Last(_context.NodePath);
                var isMetaPropertyDefinitionScope = last != null && last.Type == NodeType.MetaPropertyDefinitionScope;

                var funcCompiler = CreateFunctionCompiler(_methodNode);
                if (!funcCompiler.Function.IsStatic && memberExp == "") SuggestKeywords("this");

                // Parse the member expression
                var dte = Parser.ParseExpression(_compiler.Log, _source, memberExp, isMetaPropertyDefinitionScope ? ParseContext.MetaProperty : ParseContext.Default);

                if (isMetaPropertyDefinitionScope)
                {
                    SuggestBlock();
                }

                if (!dte.IsInvalid)
                {
                    // Resolve the member expression
                    TrySuggestMembers(funcCompiler, dte, false);
                }
                else
                {
                    if (memberExp == "")
                    {
                        foreach (string u in _context.Usings)
                        {
                            TrySuggestMembers(funcCompiler, Parser.ParseExpression(_compiler.Log, _source, u, ParseContext.Default), false);
                        }

                        SuggestTypeMembers(funcCompiler.Function.DeclaringType, AccessorLevel.Private, true, false, true, null);

                        if (!funcCompiler.Function.IsStatic)
                            SuggestTypeMembers(funcCompiler.Function.DeclaringType, AccessorLevel.Private, false, false, true, null);

                        SuggestTypes(memberExp, true, false);
                        SuggestLocals(funcCompiler);
                    }
                    else
                    {
                        SuggestTypes(memberExp, true, false);
                    }
                }
            }
        }
    }
}
