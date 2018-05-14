using Uno;
using Uno.Collections;

namespace Outracks.Simulator.Runtime
{
	using Bytecode;

	// This object is mutable because it can be captured by lambdas before all variables are bound due to function scoping
	public sealed class Environment 
	{
		readonly Dictionary<Variable, object> _variableBindings = new Dictionary<Variable, object>();

		readonly Optional<Environment> _parent;

		public Environment(Optional<Environment> parent)
		{
			_parent = parent;
		}

		public void Bind( ImmutableList<Parameter> parameters, object[] arguments)
		{
			for (int i = 0; i < parameters.Count; i++)
				Bind(parameters.Get(i).Name, arguments[i]);
		}

		public void Bind(Variable variable, object value)
		{
			_variableBindings.Add(variable, value);
		}

		public object GetValue(Variable variable)
		{
			object value;
			if (_variableBindings.TryGetValue(variable, out value))
				return value;

			if (_parent.HasValue)
				return _parent.Value.GetValue(variable);

			throw new VariableNotBound(variable);
		}
	}

	public class VariableNotBound : Exception
	{
		public readonly Variable Variable;

		public VariableNotBound(Variable variable)
			: base("Element '" + variable.Name + "' was referenced before it was initialized")
		{
			Variable = variable;
		}
	}
}