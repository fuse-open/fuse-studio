using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.IO;
using System.Linq;
using System.Xml;
using System.Xml.Linq;
using Outracks.Simulator.Protocol;
using Uno.UX.Markup;
using Uno.UX.Markup.Reflection;

namespace Outracks.Simulator.Parser
{
	public class GhostCompilerFactory
	{
		readonly IDataTypeProvider _realCompiler;
		public GhostCompilerFactory(IDataTypeProvider realCompiler)
		{
			_realCompiler = realCompiler;
		}

		/// <exception cref="InvalidMarkup"></exception>
		/// <exception cref="CyclicClassHierarchy"></exception>
		/// <exception cref="TypeNotFound"></exception>
		/// <exception cref="UnknownBaseClass"></exception>
		/// <exception cref="UnknownMemberType"></exception>
		/// <exception cref="TypeNameCollision"></exception>
		public IDataTypeProvider CreateGhostCompiler(System.Collections.Immutable.ImmutableList<UxFileContents> documents)
		{
			return new GhostCompiler(_realCompiler, CreateGhostDataTypes(documents));
		}

		/// <exception cref="InvalidMarkup"></exception>
		/// <exception cref="CyclicClassHierarchy"></exception>
		/// <exception cref="TypeNotFound"></exception>
		/// <exception cref="UnknownBaseClass"></exception>
		/// <exception cref="UnknownMemberType"></exception>
		/// <exception cref="TypeNameCollision"></exception>
		IImmutableList<IDataType> CreateGhostDataTypes(System.Collections.Immutable.ImmutableList<UxFileContents> documents)
		{
			var parsedDocuments = ParseDocuments(documents);
			var nodes = parsedDocuments.SelectMany(OuterClassNode.GetOuterClassNodes);
			var orderedNodes = OrderByDependencies(nodes);

			var generatedDataTypes = new Dictionary<string, GhostDataType>();
			foreach (var node in orderedNodes)
			{
				var n = node;
				var baseType = TryGetDataType(node.BaseTypeName, generatedDataTypes)
					.OrThrow(new UnknownBaseClass(node.BaseTypeName, n.GeneratedTypeName, n.DeclaringFile));

				generatedDataTypes[node.GeneratedTypeName] = new GhostDataType(
					n.GeneratedTypeName,
					baseType,

					globalResources: self => n.GlobalResources.Select(
						r => new GhostGlobalResource(
							fullPath: n.GeneratedTypeName + "." + r.Name,
							dataType: GetMemberDataType(r, n, generatedDataTypes),
							globalSymbol: r.Name)),

					declaredProperties: self => n.Properties.Select(
						p => new GhostProperty(
							name: p.Name,
							dataType: GetMemberDataType(p, n, generatedDataTypes),
							declaringType: self)));
			}

			foreach (var ghostType in generatedDataTypes.Values)
				ghostType.CompleteType();

			return ImmutableList.ToImmutableList<IDataType>(generatedDataTypes.Values);
		}

		/// <exception cref="UnknownMemberType"></exception>
		IDataType GetMemberDataType(IMemberNode member, OuterClassNode declaringNode, IDictionary<string, GhostDataType> generatedDataTypes)
		{
			return TryGetDataType(member.TypeName, generatedDataTypes)
				.OrThrow(new UnknownMemberType(member, declaringNode));
		}

		Optional<IDataType> TryGetDataType(string name, IDictionary<string, GhostDataType> generatedDataTypes)
		{
			return generatedDataTypes.TryGetValue(name).Select(t => (IDataType)t).Or(TryGetCompiledType(name));
		}

		Optional<IDataType> TryGetCompiledType(string typeName)
		{
			var baseDataType = _realCompiler.TryGetTypeByName(typeName);
			if (baseDataType != null)
				return Optional.Some(baseDataType);

			foreach (var ns in Configuration.DefaultNamespaces)
			{
				var bd = _realCompiler.TryGetTypeByName(ns + "." + typeName);
				if (bd != null)
					return Optional.Some(bd);
			}

			return Optional.None();
		}

		/// <exception cref="InvalidMarkup"></exception>
		static IImmutableList<ParsedDocument> ParseDocuments(System.Collections.Immutable.ImmutableList<UxFileContents> documents)
		{
			return ImmutableList.ToImmutableList(
				documents.Select(doc => new ParsedDocument(ParseXml(doc), doc.Path)));
		}

		/// <exception cref="InvalidMarkup"></exception>
		static XElement ParseXml(UxFileContents document)
		{
			try
			{
				using (var stream = ToMemoryStream(document.Contents))
					return XmlHelpers.ReadAllXml(stream, LoadOptions.SetLineInfo, true).Root;
			}
			catch (XmlException e)
			{
				throw new InvalidMarkup(
					document.Path,
					new TextPosition(new LineNumber(e.LineNumber + 1), new CharacterNumber(e.LinePosition + 1)),
					e.Message);
			}
			catch (MarkupException e)
			{
				throw new InvalidMarkup(
					document.Path,
					Optional.None(),
					e.Message);
			}
		}

		static Stream ToMemoryStream(string data)
		{
			var stream = new MemoryStream();
			var writer = new StreamWriter(stream);
			writer.Write(data);
			writer.Flush();
			stream.Position = 0;
			return stream;
		}

		/// <exception cref="TypeNameCollision"></exception>
		/// <exception cref="CyclicClassHierarchy"></exception>
		static IEnumerable<OuterClassNode> OrderByDependencies(IEnumerable<OuterClassNode> unorderedNodes)
		{
			var nodes = MapNodesByName(unorderedNodes);

			var visited = new Dictionary<OuterClassNode, bool>();
			var sorted = new List<OuterClassNode>();

			Action<OuterClassNode> visit = null;
			visit = (item) =>
			{
				bool inProcess;
				var alreadyVisited = visited.TryGetValue(item, out inProcess);

				if (alreadyVisited && inProcess)
					throw new CyclicClassHierarchy();

				if (!alreadyVisited)
				{
					visited[item] = true;

					OuterClassNode dependency;
					if (nodes.TryGetValue(item.BaseTypeName, out dependency))
						visit(dependency);

					visited[item] = false;
					sorted.Add(item);
				}
			};

			foreach (var item in nodes.Values)
				visit(item);

			return sorted;
		}

		/// <exception cref="TypeNameCollision"></exception>
		static Dictionary<string, OuterClassNode> MapNodesByName(IEnumerable<OuterClassNode> unorderedNodes)
		{
			var nodes = new Dictionary<string, OuterClassNode>();
			foreach (var n in unorderedNodes)
			{
				if (nodes.ContainsKey(n.GeneratedTypeName))
					throw new TypeNameCollision(n.GeneratedTypeName);

				nodes.Add(n.GeneratedTypeName, n);
			}
			return nodes;
		}
	}
}