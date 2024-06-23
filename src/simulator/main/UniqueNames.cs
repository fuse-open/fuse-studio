using System.Collections.Generic;
using System.Collections.Immutable;
using Outracks.Simulator.Bytecode;
using Outracks.Simulator.CodeGeneration;
using Uno.UX;
using Uno.UX.Markup.UXIL;

namespace Outracks.Simulator
{
	public class UniqueNames
	{
		readonly ImmutableDictionary<object, Variable> _names;
		readonly ImmutableDictionary<Variable, object> _namesInverse;

		public UniqueNames()
			: this(
				ImmutableDictionary<object, Variable>.Empty,
				ImmutableDictionary<Variable, object>.Empty)
		{ }

		UniqueNames(
			ImmutableDictionary<object, Variable> names,
			ImmutableDictionary<Variable, object> namesInverse)
		{
			_names = names;
			_namesInverse = namesInverse;
		}

		public Variable this[Node node]
		{
			get { return _names[node]; }
		}

		public UniqueNames GenerateNames(IEnumerable<Node> nodes)
		{
			var names = _names.ToBuilder();
			var usedNames = _namesInverse.ToBuilder();
			foreach (var node in nodes)
			{
				if (!names.ContainsKey(node))
				{
					var name = GetName(node, usedNames);
					// Beware! we might hide names, but that's how ux works
					usedNames[name] = node;
					names[node] = name;
				}
			}
			return new UniqueNames(names.ToImmutable(), usedNames.ToImmutable());
		}

		Variable GetName(Node node, IDictionary<Variable, object> usedNames)
		{
			var cn = node as ClassNode;
			if (cn != null)
				return cn.GetUxConstructorFunctionName();

			var rn = node as NewObjectNode;
			if (rn != null && rn.InstanceType == InstanceType.Global)
				return new Variable(rn.Scope.GeneratedClassName + "." + rn.Name);

			var bvn = node as BoxedValueNode;
			if (bvn != null && bvn.InstanceType == InstanceType.Global)
				return new Variable(bvn.Scope.GeneratedClassName + "." + bvn.Name);

			if (rn != null && string.IsNullOrEmpty(rn.Name) == false)
				return new Variable(rn.Name);

			return GetUniqueName(usedNames);
		}


		public Variable GetUniqueName()
		{
			return GetUniqueName(_namesInverse);
		}

		static Variable GetUniqueName(IDictionary<Variable, object> usedNames)
		{
			for (int index = usedNames.Count; ; index++)
			{
				var tryName = new Variable(index + "#");
				if (!usedNames.ContainsKey(tryName))
					return tryName;
			}
		}

		public UniqueNames Reserve(Variable variable)
		{
			var obj = new object();
			return new UniqueNames(_names.SetItem(obj, variable), _namesInverse.SetItem(variable, obj));
		}

		public UniqueNames Add(Node fn, Variable parameter)
		{
			return new UniqueNames(_names.SetItem(fn, parameter), _namesInverse.SetItem(parameter, fn));
		}

		public bool Contains(Node node)
		{
			return _names.ContainsKey(node);
		}

		public bool Contains(Variable var)
		{
			return _namesInverse.ContainsKey(var);
		}
	}
}