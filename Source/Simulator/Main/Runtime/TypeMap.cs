using System;
using System.Linq;
using System.Reflection;
using Outracks.Simulator.Bytecode;
using Outracks.Simulator.Runtime;

namespace Outracks.Simulator.Client
{
	public interface ITypeAliasNameResolver
	{
		string Resolve(string typeName);
	}

	public class TypeMap : ITypeMap
	{
		readonly ITypeAliasNameResolver _aliases;
		readonly Assembly[] _assemblies;

		public TypeMap(
			ITypeAliasNameResolver aliases,
			params Assembly[] assemblies)
		{
			_aliases = aliases;
			_assemblies = assemblies.ToArray();
		}

		public Type ResolveType(string typeNameString)
		{
			var typeName = TypeName.Parse(typeNameString);

			if (typeName.IsParameterizedGenericType)
				return TryResolveGenericType(typeName);

			return AntialiasAndLoad(typeName);
		}

		Type TryResolveGenericType(TypeName typeName)
		{
			var genericType = AntialiasAndLoad(typeName.WithGenericSuffix);
			var typeArguments = typeName.GenericArgumentsRecursively.Select(AntialiasAndLoad);

			return genericType.MakeGenericType(typeArguments.ToArray());
		}

		Type AntialiasAndLoad(TypeName typeName)
		{
			var resolvingTypeName = typeName.IsParameterizedGenericType
				? typeName.WithGenericSuffix
				: typeName;
			return LoadType(Antialias(resolvingTypeName));
		}

		Type LoadType(TypeName typeName)
		{
			foreach (var ass in _assemblies)
			{
				var type = ass.GetType(typeName.FullName);
				if (type != null)
					return type;
			}

			throw new TypeNotFound(typeName.FullName);
		}

		TypeName Antialias(TypeName typeName)
		{
			return TypeName.Parse(_aliases.Resolve(typeName.FullName));
		}
	}
}