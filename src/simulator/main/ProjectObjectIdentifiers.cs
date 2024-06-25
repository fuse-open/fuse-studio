using System;
using System.Collections.Immutable;
using Uno.UX.Markup.Reflection;
using Uno.UX.Markup.UXIL;

namespace Outracks.Simulator
{
	public class ProjectObjectIdentifiers
	{
		public static ProjectObjectIdentifiers Create(Project project, IDataTypeProvider typeInfo, Action<Exception> onError)
		{
			var idToNode = ImmutableDictionary.CreateBuilder<ObjectIdentifier, Node>();
			var nodeToId = ImmutableDictionary.CreateBuilder<Node, ObjectIdentifier>();


			foreach (var kp in project.Documents)
			{
				var path = kp.Key;
				var document = kp.Value;

				try
				{
					var index = 0;
					foreach (var node in document.NodesInDocumentOrder)
					{
						var id = new ObjectIdentifier(path, index);
						idToNode[id] = node;
						nodeToId[node] = id;
						index++;
					}
				}
				catch (Exception e)
				{
					onError(e);
				}
			}

			return new ProjectObjectIdentifiers
			{
				_idToNode = idToNode.ToImmutable(),
				_nodeToId = nodeToId.ToImmutable(),
			};
		}

		ImmutableDictionary<ObjectIdentifier, Node> _idToNode;
		ImmutableDictionary<Node, ObjectIdentifier> _nodeToId;

		public Optional<Node> TryGetNode(ObjectIdentifier id)
		{
			Node node;
			if (_idToNode.TryGetValue(id, out node))
				return node;

			return Optional.None();
		}

		public Optional<ObjectIdentifier> TryGetIdentifier(Node node)
		{
			ObjectIdentifier id;
			if (_nodeToId.TryGetValue(node, out id))
				return id;

			return Optional.None();
		}
	}
}