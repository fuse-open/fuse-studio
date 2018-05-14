using System.Collections.Generic;
using Uno.Compiler.API.Domain;

namespace Outracks.Simulator.Client
{

	public class TypeAliasNameResolver2 : ITypeAliasNameResolver
	{
		readonly IDictionary<string, string> _cilTypeMap;

		public TypeAliasNameResolver2(IDictionary<string, string> cilTypeMap)
		{
			_cilTypeMap = cilTypeMap;
		}

		public string Resolve(string typeName)
		{
			string aliasedTypeName;
			if (!TypeAliases.TryGetAliasFromType(typeName, out aliasedTypeName))
				aliasedTypeName = typeName;

			//if (aliasedTypeName.Contains("`"))
			//	aliasedTypeName = aliasedTypeName.BeforeFirst("`") + "<" + new string(',', int.Parse(aliasedTypeName.AfterFirst("`"))-1) + ">";

			string cilType;
			if (_cilTypeMap.TryGetValue(aliasedTypeName, out cilType))
				return cilType;

			return typeName;

		}
	}
}