using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Reflection;
using Outracks.Simulator.Bytecode;
using Outracks.Simulator.UXIL;
using Uno.Collections;
using Uno.Compiler.API.Domain.IL;
using Uno.UX.Markup.Reflection;
using Uno.UX.Markup.UXIL;

namespace Outracks.Simulator
{
	public static class MetadataGenerator
	{
		public static ProjectMetadata GenerateMetadata(
			this Project project,
			ProjectObjectIdentifiers ids)
		{
			var hierarchy = new ConcurrentDictionary<ObjectIdentifier, IEnumerable<ObjectIdentifier>>();
			var precompiledElements = new List<PrecompiledElement>();

			var nodeWorklist = new ConcurrentQueue<Node>();
			var typeWorklist = new ConcurrentQueue<DataType>();

			foreach (var doc in project.RootClasses)
				nodeWorklist.Enqueue(doc.Root);

			Node node;
			while (nodeWorklist.TryDequeue(out node))
			{
				AddMetadata(node, ids, hierarchy,
					dt =>
					{
						var uxClass = dt as ClassNode;
						if (uxClass != null)
							return ids
								.TryGetIdentifier(uxClass)
								.Select(id => new ObjectIdentifier(id.ToString()))
								.ToList();

						return TryGetUnoDataType(dt)
							.MatchWith(
								none: () => ImmutableList<ObjectIdentifier>.Empty,
								some: unoDataType =>
								{
									typeWorklist.Enqueue(unoDataType);

									return List.Create(new ObjectIdentifier(unoDataType.FullName));
								});
					});

				foreach (var child in node.Children)
					nodeWorklist.Enqueue(child);
			}

			DataType type;
			while (typeWorklist.TryDequeue(out type))
			{
				if (IsUxDeclarable(type))
					precompiledElements.Add(new PrecompiledElement(
						id: new ObjectIdentifier(type.FullName),
						source:
							"<" + type.Base.FullName + " ux:Class=\""+ type.FullName +"\">" +
								type.Properties
									.Where(p => IsUxDeclarable(p.ReturnType))
									.Select(p => "<" + p.ReturnType.FullName + " ux:Property=\"" + p.Name + "\" />")
									.Join("") +
							"</" + type.Base.FullName + ">"));

				hierarchy.AddOrUpdate(
					key: new ObjectIdentifier(type.FullName),
					addValueFactory: _ =>
					{
						var baseType = type.Base;
						if (baseType == null)
							return ImmutableList<ObjectIdentifier>.Empty;

						typeWorklist.Enqueue(baseType);
						return List.Create(new ObjectIdentifier(baseType.FullName));
					},
					updateValueFactory: (_, v) => v);
			}

			return new ProjectMetadata(hierarchy.ToImmutableList(), precompiledElements.ToImmutableList());
		}

		static bool IsUxDeclarable(DataType type)
		{
			return IsUxLike(type)
				&& type.Base != null
				&& IsUxLike(type.Base);
		}

		static bool IsUxLike(DataType type)
		{
			return !type.IsGenericType && !type.IsGenericParameterization && !type.IsGenericDefinition && !type.IsArray;
		}

		public static Optional<DataType> TryGetUnoDataType(IDataType type)
		{
			if (type == null)
				return Optional.None();

			var tryGetUnoTypeByName = TryGetUnoTypeByNameFactory(type);

			var unoType = tryGetUnoTypeByName(type.QualifiedName);
			if (unoType == null)
				return Optional.None();

			return unoType;
		}

		static Func<string, DataType> TryGetUnoTypeByNameFactory(IDataType type)
		{
			type = type.ActualIDataTypeImpl ?? type;

			var cField = type.GetType().GetField("_c", BindingFlags.NonPublic | BindingFlags.Instance);
			if (cField == null) // not available on deferred generic type etc
				return name => null;

			var c = cField.GetValue(type) as CompilerDataTypeProvider;
			if (c == null)
				throw new Exception("UXIL private field hack broke");

			var tryGetUnoTypeByNameMethod = c.GetType().GetMethod("TryGetUnoTypeByName", BindingFlags.NonPublic | BindingFlags.Instance);
			if (tryGetUnoTypeByNameMethod == null)
				throw new Exception("UXIL private field hack broke");

			return name => tryGetUnoTypeByNameMethod.Invoke(c, new object[] { name }) as DataType;
		}

		static void AddMetadata(
			Node node,
			ProjectObjectIdentifiers ids,
			ConcurrentDictionary<ObjectIdentifier, IEnumerable<ObjectIdentifier>> metadata,
			Func<IDataType, ImmutableList<ObjectIdentifier>> enqueue)
		{
			ids.TryGetIdentifier(node).Do(id =>
				metadata.AddOrUpdate(
					key: id,
					addValueFactory: _ =>
						node.MatchWith(
							(DocumentScope ds) => ds.MatchWith(
								(ClassNode cn) => enqueue(cn.BaseType),
								(TemplateNode tn) => enqueue(tn.ProducedType)),
							(ObjectNode on) => enqueue(on.DataType),
							(PropertyNode pn) => ImmutableList<ObjectIdentifier>.Empty,
							(DependencyNode dn) => ImmutableList<ObjectIdentifier>.Empty),
					updateValueFactory: (_, v) => v));
		}

	}
}