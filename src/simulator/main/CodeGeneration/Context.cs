using System;
using System.Collections.Immutable;
using Outracks.Simulator.Bytecode;
using Outracks.Simulator.UXIL;
using Uno.UX.Markup.UXIL;

namespace Outracks.Simulator.CodeGeneration
{
	public sealed class Context
	{
		public readonly UniqueNames Names;
		public readonly Func<Node, Optional<ObjectIdentifier>> TryGetTagHash;
		public readonly string ProjectDirectory;

		readonly ImmutableHashSet<TypeName> _typesDeclaredInUx;

		public Context(UniqueNames names, Func<Node, Optional<ObjectIdentifier>> tryGetTagHash, string projectDirectory, ImmutableHashSet<TypeName> typesDeclaredInUx)
		{
			Names = names;
			TryGetTagHash = tryGetTagHash;
			ProjectDirectory = projectDirectory;
			_typesDeclaredInUx = typesDeclaredInUx;
		}

		public Context With(UniqueNames names = null)
		{
			return new Context(names ?? Names, TryGetTagHash, ProjectDirectory, _typesDeclaredInUx);
		}

		public Optional<Expression> TryGetUxConstructorFunction(TypeName typeName)
		{
			return IsDeclaredInUx(typeName)
				? Optional.Some<Expression>(new ReadVariable(typeName.GetUxConstructorFunctionName()))
				: Optional.None<Expression>();
		}


		public bool IsDeclaredInUx(Property property)
		{
			return IsDeclaredInUx(property.Facet.DeclaringType.GetTypeName());
		}

		bool IsDeclaredInUx(TypeName typeName)
		{
			return _typesDeclaredInUx.Contains(typeName);
		}
	}
}