using System.Collections.Generic;
using System.Linq;
using Outracks.CodeCompletion;
using Uno.Compiler.API;
using Uno.UX.Markup;

namespace Outracks.UnoDevelop.UXNinja
{
	public static class SuggestionParser
	{
		public static IEnumerable<SuggestItem> GetSuggestions(
			ICompiler compiler,
			IContext context,
			int offset,
			ICodeReader codeReader)
		{
			var parentElement = context.TargetElement == null ? null : context.TargetElement.Parent;

			foreach (var defaultNamespace in Configuration.DefaultNamespaces.Concat(new [] { "Uno" }))
			{
				if (context.NamespaceDeclarations.ContainsKey(defaultNamespace))
					continue;

				context.NamespaceDeclarations.Add(defaultNamespace, "");
			}

			var dataTypes = new DataTypes(compiler, compiledTypes => ResolveUxClasses.GetUxClassTypes(context, compiledTypes, compiler.Data.IL));

			switch (context.Type)
			{
				case ContextType.StartTagName:
					return new StartTagNameSuggestion(
						dataTypes,
						parentElement,
						context.TargetElement,
						context.NamespaceDeclarations).Suggest();

				case ContextType.StartTagAttributeName:
					return new StartTagAttributeNameSuggestion(
						dataTypes,
						context.TargetElement,
						context.NamespaceDeclarations).Suggest();

				case ContextType.StartTagAttributeValue:
					return new StartTagAttributeValueSuggestion(
						dataTypes,
						context.TargetElement,
						parentElement,
						GetCurrentAttributeContext(context.TargetElement, offset),
						codeReader,
						context.NamespaceDeclarations).Suggest();
			}

			return Enumerable.Empty<SuggestItem>();
		}

		static IAttributeContext GetCurrentAttributeContext(IElementContext targetElement, int offset)
		{
			return targetElement.Attributes.FirstOrDefault(attribute => attribute.Source.EndOffset >= offset);
		}
	}
}
