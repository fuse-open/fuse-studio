using System;
using System.Collections.Generic;
using System.Linq;
using System.Xml;
using System.Xml.Linq;
using Outracks.Simulator.Bytecode;
using Uno.UX.Markup;
using Uno.UX.Markup.Reflection;
using Uno.UX.Markup.UXIL;

namespace Outracks.Simulator.UXIL
{
	public static class Marshalling
	{
		public static IEnumerable<ValueSource> ConstructorArguments(this NewObjectNode self)
		{
				var args = self.DataType.Properties.Where(x => x.IsConstructorArgument);
				foreach (var arg in args)
				{
					var p = self.Properties.FirstOrDefault(a => a.Facet == arg);
					if (p is AtomicProperty)
					{
						var ap = (AtomicProperty)p;
						if (ap.Value != null)
						    yield return new AtomicValueSource(ap.Value);
					}
					else if (p is ReferenceProperty)
					{
						var rp = (ReferenceProperty)p;
						if (rp.Source != null)
						    yield return rp.Source;
					}
					else throw new Exception("Unhandled constructor argument type: " + p.GetType().FullName);
				}

		}

		public static Optional<TextPosition> TryGetTextPosition(this XObject element)
		{
			IXmlLineInfo info = element;
			if (!info.HasLineInfo())
				return Optional.None();

			return new TextPosition(
				new LineNumber(info.LineNumber),
				new CharacterNumber(info.LinePosition));

		}

		public static IEnumerable<Node> AllNodesInProject(this Project project)
		{
			return project.RootClasses
				.SelectMany(AllNodesInDocument)
				.Distinct();
		}

		public static IEnumerable<Node> AllNodesInDocument(this ClassNode document)
		{
			return document.AllScopes
				.SelectMany(scope => scope.NodesIncludingRoot)
				.Distinct();
		}

		public static TypeName GetTypeName(this IDataType dt)
		{
			return TypeName.Parse(dt.QualifiedName);
		}

		public static TypeMemberName GetMemberName(this Event ev)
		{
			return new TypeMemberName(ev.Facet.Name);
		}

		public static TypeMemberName GetMemberName(this Property p)
		{
			return new TypeMemberName(p.Facet.Name);
		}

		public static StaticMemberName GetStaticMemberName(this IAttachedEvent a)
		{
			return new StaticMemberName(
				a.DeclaringType.GetTypeName(),
				new TypeMemberName(a.AddMethodName));
		}


		public static bool IsAttachedProperty(this Property p)
		{
			return p.Facet is IAttachedProperty;
		}

		public static StaticMemberName GetEnumValueName(this EnumValue v)
		{
			return new StaticMemberName(
				v.Enum.GetTypeName(),
				new TypeMemberName(v.Enum.Literals.First(l => l.Value == v.LiteralIntValue).Name));
		}

		public static Optional<StaticMemberName> GetResetMethod(this Property p)
		{
			if (p.IsAttachedProperty())
				return new StaticMemberName(
					p.Facet.DeclaringType.GetTypeName(),
					new TypeMemberName("Reset" + p.Facet.Name.AfterLast(".")));

			return Optional.None();
		}


		public static bool IsSize(this Property p)
		{
			return p.Facet.DataType.FullName == "Uno.UX.Size";
		}

		public static bool IsSize2(this Property p)
		{
			return p.Facet.DataType.FullName == "Uno.UX.Size2";
		}

		public static StaticMemberName GetSetMethodName(this Property p)
		{
			var ap = (IAttachedProperty)p.Facet;
			return new StaticMemberName(
				p.Facet.DeclaringType.GetTypeName(),
				new TypeMemberName(ap.SetMethodName));
		}

		public static StaticMemberName GetGetMethodName(this Property p)
		{
			var ap = (IAttachedProperty)p.Facet;
			return new StaticMemberName(
				p.Facet.DeclaringType.GetTypeName(),
				new TypeMemberName(ap.GetMethodName));
		}

		public static StaticMemberName GetPropertyObjectField(this Property p)
		{
			return new StaticMemberName(
				p.Facet.DeclaringType.GetTypeName(),
				new TypeMemberName(p.Facet.Name + "Property"));
		}

		public static IEnumerable<Property> SinglePropertiesWithValues(this Node node)
		{
			return Enumerable.Union<Property>(
				node.MutableAtomicPropertiesWithValues,
				node.MutableReferencePropertiesWithValues);
		}

		public static TypeName ToTypeName(this TypeNameHelper tnh)
		{
			return TypeName.Parse(tnh.FullName);
		}
	}
}