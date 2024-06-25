using System;
using System.Linq;
using Outracks.UnoDevelop.CodeNinja.AmbientParser;
using Uno;
using Uno.Compiler.API.Domain.IL.Types;
using Uno.Compiler.Core;
using Uno.Compiler.Core.Syntax.Binding;
using Uno.Compiler.Core.Syntax.Compilers;
using Uno.Compiler.Frontend.Analysis;
using Uno.Logging;

namespace Outracks.UnoDevelop.CodeNinja
{
    public static class GoToDefinition
    {
		public static SourceObject TryGetSourceObject(Compiler compiler, Source src, IUnoCodeReader codeReader, ParseResult parseResult)
        {
            try
            {
                int start, length;
                string memberExp;
				return Compute(compiler, src, codeReader, parseResult, out start, out length, out memberExp);
            }
            catch (MaxErrorException)
            {
                return null;
            }
        }

        public static SourceObject Compute(
            Compiler compiler,
            Source src,
            IUnoCodeReader codeReader,
            ParseResult parseResult,
            out int rangeStart,
            out int rangeLength,
            out string memberExp)
        {
            var cc = new CodeCompleter.CodeCompleter(compiler, src, codeReader, codeReader.Offset, parseResult);
            return Compute(src, codeReader, cc, out rangeStart, out rangeLength, out memberExp);
        }

        static SourceObject Compute(
            Source src,
            IUnoCodeReader codeReader,
            CodeCompleter.CodeCompleter codeCompleter,
            out int rangeStart,
            out int rangeLength,
            out string memberExp)
        {
            memberExp = "";
            rangeStart = 0;
            rangeLength = 0;
            try
            {
                memberExp = FindMemberExpression(codeReader, out rangeStart, out rangeLength, codeCompleter);

                if (memberExp == null || codeCompleter.Context.NodePath == null)
                    return null;

                var methodNode = codeCompleter.Context.NodePath.LastOrDefault(x => NodeTypeHelpers.IsMethodNode(x.Type));

                var fc = codeCompleter.CreateFunctionCompiler(methodNode);
                var pe = codeCompleter.ResolveStringInFunctionContext(methodNode, memberExp, src, fc);

				if ((pe == null || pe.IsInvalid) && fc.Namescope != null)
				{
					var compute = ResolveStringWhenClassDeclaration(memberExp, fc);
					if (compute != null)
						return compute;
				}

	            return ResolvePartialExpression(pe);
            }
            catch (Exception e)
            {
                if (e is MaxErrorException) throw;
                return null;
            }
        }

		static SourceObject ResolveStringWhenClassDeclaration(string memberExp, FunctionCompiler fc)
	    {
		    var classScope = fc.Namescope as ClassType;
			if (classScope == null) return null;

			var found = classScope.Properties.FirstOrDefault(property => property.Name == memberExp) as SourceObject;
			if (found != null)
				return found;

			found = classScope.Fields.FirstOrDefault(field => field.Name == memberExp);
			if (found != null)
				return found;

			found = classScope.Methods.FirstOrDefault(method => method.Name == memberExp);
			if (found != null)
				return found;

			return null;
	    }

	    public static SourceObject ResolvePartialExpression(PartialExpression pe)
        {
            if (pe == null) return null;

            switch (pe.ExpressionType)
            {
                case PartialExpressionType.Namespace:
                    return ((PartialNamespace)pe).Namespace;

                case PartialExpressionType.Type:
                    return ((PartialType)pe).Type;

                case PartialExpressionType.Block:
                    return ((PartialBlock)pe).Block;

                case PartialExpressionType.Value:
                    return ((PartialValue)pe).Value;

                case PartialExpressionType.Variable:
                    return ((PartialVariable)pe).Variable;

                case PartialExpressionType.Parameter:
                    {
                        var pp = (pe as PartialParameter);
						return pp.Function.Match(f => f.Parameters, l => l.Parameters)[pp.Index];
                    }

                case PartialExpressionType.Field:
                    return ((PartialField)pe).Field;

                case PartialExpressionType.Property:
                    return ((PartialProperty)pe).Property;

                case PartialExpressionType.Event:
                    return ((PartialEvent)pe).Event;

                case PartialExpressionType.Indexer:
                    return ((PartialIndexer)pe).Indexer;

                case PartialExpressionType.ArrayElement:
                    return ((PartialArrayElement)pe).Object;

                case PartialExpressionType.MethodGroup:
                    // TODO: resolve overload based on argument list (if available) to resolve correct method;
                    return ((PartialMethodGroup)pe).Methods[0];

                default:
                    return null;
            }
        }

        public static string FindMemberExpression(
            IUnoCodeReader codeReader,
            out int rangeStart,
            out int rangeLength,
            CodeCompleter.CodeCompleter cc)
        {
            rangeStart = codeReader.Offset;
            rangeLength = codeReader.Offset;

            // Skip to end of identifier;
            while (true)
            {
                var cs = codeReader.PeekText(1);
                if (string.IsNullOrEmpty(cs)) return null;
                var c = cs[0];
                if (char.IsLetterOrDigit(c) || c == '_') codeReader.ReadText(1);
                else break;
            }
            int rangeEnd = codeReader.Offset;

            if (cc.Context.NodePath == null) return null;

            TokenType tt;
            var memberExp = cc.FindMemberExpression(out tt, false, false);

            rangeStart = rangeEnd - memberExp.Length;
            rangeLength = memberExp.Length;

            return memberExp;
        }
    }
}
