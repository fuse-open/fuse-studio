using System.Linq;
using Outracks.IO;
using Uno.Compiler.API;
using Outracks.UnoDevelop.UXNinja;
using Uno;
using Uno.UX.Markup;

namespace Outracks.CodeCompletionFactory.UXNinja
{
	public static class SourceEntityFactoryUX
	{
		public static SourceObject GetDataTypeFromOffset(AbsoluteFilePath filePath, string code, int textOffset, ICompiler compiler)
		{
			var context = Context.CreateContext(filePath, code, textOffset, compiler.Input.Package);

			foreach (var defaultNamespace in Configuration.DefaultNamespaces)
			{
				if (context.NamespaceDeclarations.ContainsKey(defaultNamespace)) continue;
				context.NamespaceDeclarations.Add(defaultNamespace, "");
			}

			var targetElement = GetTargetElement(context, textOffset);
			if (targetElement == null) 
				return null;

			var dataType = targetElement.ToDataType(compiler.Utilities.FindAllTypes(), context.NamespaceDeclarations);				
			if (dataType == null) 
				return null;

			var properties = PropertyHelper.GetAllWriteableProperties(dataType).ToList();
			foreach (var attribute in targetElement.Attributes.Where(attribute => 
				textOffset >= attribute.Source.Offset && textOffset <= attribute.Source.EndOffset))
			{
				return properties.FirstOrDefault(p => p.Name == attribute.Name);
			}

			return dataType;
		}

		public static IElementContext GetTargetElement(IContext context, int offset)
		{
			return context.Root.TraverseSelfAndAllChildren()
				.FirstOrDefault(element => IsOffsetInsideElement(element, offset));
		}

		public static bool IsOffsetInsideElement(IElementContext element, int offset)
		{
			var isInsideStartTag = element.StartTagSource.Offset <= offset && element.StartTagSource.EndOffset >= offset;
			var isInsideEndTag = element.EndTagSource.Offset <= offset && element.EndTagSource.EndOffset >= offset;
			return isInsideStartTag || isInsideEndTag;
		}
	}
}