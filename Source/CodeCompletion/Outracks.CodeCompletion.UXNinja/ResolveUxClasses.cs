using System.Collections.Generic;
using System.Linq;
using Uno.Compiler.API.Domain;
using Uno.Compiler.API.Domain.IL;
using Uno.Compiler.API.Domain.IL.Members;
using Uno.Compiler.API.Domain.IL.Types;

namespace Outracks.UnoDevelop.UXNinja
{
	static class ResolveUxClasses 
	{
		public static IEnumerable<DataType> GetUxClassTypes(IContext context, IEnumerable<DataType> dataTypes, Namespace rootNamespace)
		{
			if (context.Root == null)
				return Enumerable.Empty<DataType>();

			var uxClasses = context.Root
				.TraverseAllChildren()
				.Where(elm => elm.HasAttributeWithNamespace("ux", "Class"))
				.Select(elm =>
				{
					var className = elm.GetAttributeValue("ux", "Class");
					var ns = GetParentNamespaceForUxClassName(className, rootNamespace);

					var fakeType = new ClassType(elm.StartTagSource, ns, "", Modifiers.Generated | Modifiers.Public, className);
					return new { Type = fakeType, Element = elm };
				})
				.ToArray();

			foreach (var klass in uxClasses)
			{
				// ReSharper disable once PossibleMultipleEnumeration
				var types = dataTypes.Concat(uxClasses.Select(c => c.Type));
				klass.Type.SetBase(klass.Element.ToDataType(types, context.NamespaceDeclarations));

				var properties = klass.Element.TraverseAllChildren()
					.Where(x => x.HasAttributeWithNamespace("ux", "Property"));
				klass.Type.Properties.AddRange(properties.Select(prop =>
				{
					var propertyName = prop.GetAttributeValue("ux", "Property");
					var propType = prop.ToDataType(types, context.NamespaceDeclarations) ?? DataType.Void;

					return new Property(
						prop.StartTagSource,
						"",
						Modifiers.Public | Modifiers.PropertyMember,
						propertyName,
						klass.Type,
						propType);
				}));
			}

			return uxClasses.Select(c => c.Type);
		}

		static Namespace GetParentNamespaceForUxClassName(string className, Namespace rootNamespace)
		{
			var ns = rootNamespace;

			var namespacePath = GetQualifiedNamespacePartsForString(className);
			foreach (var segment in namespacePath)
			{
				ns = ns.Namespaces.FirstOrDefault(x => x.Name == segment) ?? new Namespace(ns, segment);
			}

			return ns;
		}

		static IEnumerable<string> GetQualifiedNamespacePartsForString(this string name)
		{
			return name.Contains('.')
				? name.Substring(0, name.LastIndexOf('.')).Split('.')
				: Enumerable.Empty<string>();
		}
	}
}