using System;
using System.Collections.Generic;
using System.Linq;
using Uno.Compiler.Core.Syntax.Binding;
using Uno.Compiler.Frontend.Analysis;
using Outracks.UnoDevelop.CodeNinja.AmbientParser;
using Uno;
using Uno.Compiler.API.Domain.IL;
using Uno.Compiler.API.Domain.IL.Members;
using Uno.Compiler.API.Domain.IL.Types;

namespace Outracks.UnoDevelop.CodeNinja
{
    public static class ParameterInfoSuggester
    {
        static void FindArgListStart(IUnoCodeReader codeReader, out int currentArgIndex)
        {
            int parenCount = 0;
            currentArgIndex = 0;

            while (true)
            {
                var c = codeReader.ReadTokenReverse();
                switch (c)
                {
                    case TokenType.EndOfFile:
                        return;

                    case TokenType.LeftParen:
                        if (parenCount == 0) return;
                        parenCount--;
                        break;

                    case TokenType.RightParen:
                        parenCount++; 
                        break;

                    case TokenType.Comma:
                        if (parenCount == 0) currentArgIndex++;
                        break;
                }
            }
        }

        public static void Suggest(Uno.Compiler.Core.Compiler compiler, AmbientParser.ParseResult parseResult, IUnoCodeReader codeReader, Source src, List<Function> pinfo, out int highlightArgument, out int range, out IEnumerable<string> usings)
        {
            highlightArgument = 0;
            range = 0;

            usings = new List<string>();

            try
            {
                // Scan backwards to find function argument list start
                var caret = codeReader.Offset;
                FindArgListStart(codeReader, out highlightArgument);
                range = caret - codeReader.Offset;


                TokenType tt;
                var cc = new CodeCompleter.CodeCompleter(compiler, src, codeReader, caret, parseResult);
                
                var memberExp = cc.FindMemberExpression(out tt, false, false);

                if (tt == TokenType.Import)
                {
                    memberExp += "Importer";
                }
                else if (memberExp == "sample")
                {
                    pinfo.Add(new Method(src, null, null, 0, "sample", compiler.ILFactory.Essentials.Float4, new[]
                    {
                        new Parameter(src, null, 0, compiler.ILFactory.Essentials.Texture2D, "texture", null),
                        new Parameter(src, null, 0, compiler.ILFactory.Essentials.Float2, "texCoord", null),
                    }, null));

                    pinfo.Add(new Method(src, null, null,  0, "sample", compiler.ILFactory.Essentials.Float4, new[]
                    {
                        new Parameter(src, null, 0, compiler.ILFactory.Essentials.Texture2D, "texture", null),
                        new Parameter(src, null, 0, compiler.ILFactory.Essentials.Float2, "texCoord", null),
                        new Parameter(src, null, 0, compiler.ILFactory.Essentials.SamplerState, "samplerState", null),
                    }, null));

                    pinfo.Add(new Method(src, null, null, 0, "sample", compiler.ILFactory.Essentials.Float4, new[]
                    {
                        new Parameter(src, null, 0, compiler.ILFactory.Essentials.TextureCube, "texture", null),
                        new Parameter(src, null, 0, compiler.ILFactory.Essentials.Float3, "texCoord", null),
                    }, null));

                    pinfo.Add(new Method(src, null, null, 0, "sample", compiler.ILFactory.Essentials.Float4, new[]
                    {
                        new Parameter(src, null, 0, compiler.ILFactory.Essentials.TextureCube, "texture", null),
                        new Parameter(src, null, 0, compiler.ILFactory.Essentials.Float3, "texCoord", null),
                        new Parameter(src, null, 0, compiler.ILFactory.Essentials.SamplerState, "samplerState", null),
                    }, null));
                }
                else if (memberExp == "vertex_attrib")
                {
                    var gpt = new GenericParameterType(src, compiler.ILFactory.Essentials.Object, "T");
                    var gptarray = compiler.TypeBuilder.GetArray(gpt);
                    var ushortArray = compiler.TypeBuilder.GetArray(compiler.ILFactory.Essentials.UShort);

                    pinfo.Add(new Method(src, null, null, 0, "vertex_attrib", gpt, new[]
                    {
                        new Parameter(src, null, 0, gptarray, "vertexArray", null)
                    }, null));

                    pinfo.Add(new Method(src, null, null, 0, "vertex_attrib", gpt, new[]
                    {
                        new Parameter(src, null, 0, gptarray, "vertexArray", null),
                        new Parameter(src, null, 0, ushortArray, "indexArray", null),
                    }, null));

                    pinfo.Add(new Method(src, null, null, 0, "vertex_attrib<T>", gpt, new[]
                    {
                        new Parameter(src, null, 0, compiler.ILFactory.Essentials.VertexAttributeType, "vertexAttributeType", null),
                        new Parameter(src, null, 0, compiler.ILFactory.Essentials.VertexBuffer, "vertexBuffer", null),
                        new Parameter(src, null, 0, compiler.ILFactory.Essentials.Int, "stride", null),
                        new Parameter(src, null, 0, compiler.ILFactory.Essentials.Int, "offset", null),
                    }, null));

                    pinfo.Add(new Method(src, null, null, 0, "vertex_attrib<T>", gpt, new[]
                    {
                        new Parameter(src, null, 0, compiler.ILFactory.Essentials.VertexAttributeType, "vertexAttributeType", null),
                        new Parameter(src, null, 0, compiler.ILFactory.Essentials.VertexBuffer, "vertexBuffer", null),
                        new Parameter(src, null, 0, compiler.ILFactory.Essentials.Int, "stride", null),
                        new Parameter(src, null, 0, compiler.ILFactory.Essentials.Int, "offset", null),
                        new Parameter(src, null, 0, compiler.ILFactory.Essentials.IndexType, "indexType", null),
                        new Parameter(src, null, 0, compiler.ILFactory.Essentials.IndexBuffer, "indexBuffer", null),
                    }, null));
                }
                
                usings = cc.Context.Usings;

                if (cc.Context.NodePath == null || cc.Context.NodePath.Count == 0) return;

                var pe = cc.ResolveStringInFunctionContext(cc.Context.NodePath.LastOrDefault(x => AmbientParser.NodeTypeHelpers.IsMethodNode(x.Type)), memberExp, src);

                

                if (pe != null)
                {
                    DataType dt = null;
                    string methodName = null;

                    if (pe.ExpressionType == PartialExpressionType.MethodGroup)
                    {
                        var mg = pe as PartialMethodGroup;
                        
                        if (mg.Object != null) 
                            dt = mg.Object.ReturnType;
                        else 
                            dt = mg.Methods[0].DeclaringType;

                        methodName = mg.Methods[0].Name;
                    }
                    else if (pe.ExpressionType == PartialExpressionType.Type)
                    {
                        var pdt = pe as PartialType;

                        foreach (var c in pdt.Type.Constructors)
                        {
                            pinfo.Add(c);
                        }
                        return;
                    }

                    var hidden = new HashSet<Method>();

                    while (dt != null)
                    {
                        foreach (var m in dt.Methods)
                        {
                            if (!hidden.Contains(m))
                            {
                                if (m.Name == methodName)
                                {
                                    pinfo.Add(m);
                                }
                            }
                            if (m.OverriddenMethod != null) 
                                hidden.Add(m.OverriddenMethod);
                        }

                        dt = dt.Base;
                    }
                }
            }
            catch (Exception) {  }
        }
    }
}
