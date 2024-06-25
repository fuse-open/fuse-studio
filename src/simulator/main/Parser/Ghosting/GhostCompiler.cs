using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using Uno.UX.Markup.Reflection;

namespace Outracks.Simulator.Parser
{
	public class GhostCompiler : IDataTypeProvider
	{
		readonly IDataTypeProvider _baseCompiler;
		readonly IImmutableList<IDataType> _ghostTypes;
		readonly Dictionary<string, IDataType> _ghostTypeDict = new Dictionary<string, IDataType>();
		readonly Dictionary<string, IDataType> _nameToTypeCache = new Dictionary<string, IDataType>(); // optimization

		public GhostCompiler(IDataTypeProvider baseCompiler, IImmutableList<IDataType> ghostTypes)
		{
			_baseCompiler = baseCompiler;
			_ghostTypes = ghostTypes;
			foreach (var t in ghostTypes) _ghostTypeDict.Add(t.FullName, t);
		}

		public IDataType TryGetTypeByName(string name)
		{
			IDataType res;
			if (!_nameToTypeCache.TryGetValue(name, out res))
			{
				if (!_ghostTypeDict.TryGetValue(name, out res))
					res = _baseCompiler.TryGetTypeByName(name);
				if (res != null) _nameToTypeCache.Add(name, res);
			}
			return res;
		}

		public IDataType GetTypeByGenericAlias(string alias)
		{
			return _ghostTypes.FirstOrDefault(d => d.AutoGenericInfo != null && d.AutoGenericInfo.Alias == alias) ?? _baseCompiler.GetTypeByGenericAlias(alias);
		}

		public IDataType GetTypeByValueBindingAlias(string alias)
		{
			if (alias == null)
				alias = "Data"; // TODO: Was found in the CompilerReflection implementation, i don't know if it is needed. It may be a workaround for empty string...anyways, wut?

			return _ghostTypes.FirstOrDefault(d => d.ValueBindingInfo != null && d.ValueBindingInfo.Alias == alias) ?? _baseCompiler.GetTypeByValueBindingAlias(alias);
		}

		public IDataType GetAttachedPropertyTypeByName(string name)
		{
			return _baseCompiler.GetAttachedPropertyTypeByName(name);
		}

		public IEnumerable<IDataType> DataTypes
		{
			get { return _ghostTypes.Concat(_baseCompiler.DataTypes.Except(_ghostTypes, new DataTypeNameEquality())); }
		}
	}

	class DataTypeNameEquality : IEqualityComparer<IDataType>
	{
		public bool Equals(IDataType x, IDataType y)
		{
			return x.FullName == y.FullName;
		}

		public int GetHashCode(IDataType obj)
		{
			return obj.FullName.GetHashCode();
		}
	}
}
