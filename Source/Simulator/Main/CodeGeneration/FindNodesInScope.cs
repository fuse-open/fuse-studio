using System;
using System.Collections.Generic;
using System.Linq;
using Outracks.Simulator.UXIL;
using Uno.UX;
using Uno.UX.Markup.UXIL;

namespace Outracks.Simulator.CodeGeneration
{
	static class FindNodesInScope
	{
		// Local scopes
		public static Node[] NodesToInitializeInScope(this DocumentScope scope)
		{
			return scope
				.NodesDeclaredInScope().Where(n => n is NewObjectNode || n is NameTableNode)
				.Concat(new [] { scope })
				.ToArray();
		}

		public static Node[] NodesDeclaredInScope(this DocumentScope scope)
		{
			return scope.InstantiationOrder
				.OfType<NodeSource>()
				.Select(n => n.Node)
				.OrderByDescending(IsInnerClassDeclaration)
				.Where(n => IsDeclaredInScope(n, scope))
				.ToArray();
		}
		
		static bool IsDeclaredInScope(Node n, DocumentScope scope)
		{
			return 
				n != scope && // not the scope node itself
				n.InstanceType != InstanceType.Global &&
				(n.Scope == scope || IsInnerClassDeclaration(n));  
		}

		static bool IsInnerClassDeclaration(Node n)
		{
			return n is TemplateNode || (n is ClassNode && ((ClassNode)n).IsInnerClass == true);
		}

		// Global scope

		public static Node[] NodesToInitializeInScope(this Project scope)
		{
			return scope
				.NodesDeclaredInScope()
				.Distinct()
				.Where(n => n is NewObjectNode || n is BoxedValueNode)
				.ToArray();
		}

		public static Node[] NodesDeclaredInScope(this Project globalScope)
		{
			return globalScope.RootClasses.SelectMany(doc => doc.GlobalNodes().Concat(new [] { doc })).ToArray();
		}

		static IEnumerable<Node> GlobalNodes(this ClassNode document)
		{
			return document
				.AllNodesInDocument()
				.Where(IsGlobalDeclaration);
		}
		
		public static bool IsGlobalDeclaration(this Node n)
		{
			return n.InstanceType == InstanceType.Global;
		}

		public static bool IsOuterClassDeclaration(this Node n)
		{
			return n is ClassNode && ((ClassNode)n).IsInnerClass == false;
		}
	}
}