using System;
using System.Collections.Generic;
using System.Linq;
using Uno.UX.Markup.Reflection;

namespace Outracks.Simulator.Parser
{
	public class CachingCompiler : IDataTypeProvider
	{
		readonly IDataTypeProvider _baseCompiler;
		readonly IDataType[] _dataTypes;
		public CachingCompiler(IDataTypeProvider baseCompiler)
		{
			_baseCompiler = baseCompiler;
			_dataTypes = baseCompiler.DataTypes.Select(Cache).ToArray();
		}

		readonly Dictionary<string, IDataType> _nameToType = new Dictionary<string, IDataType>(); 
		public IDataType TryGetTypeByName(string name)
		{
			IDataType dt;
			if (!_nameToType.TryGetValue(name, out dt))
				_nameToType[name] = dt = Cache(_baseCompiler.TryGetTypeByName(name));
			return dt;
		}

		readonly Dictionary<string, IDataType> _genericAliasToType = new Dictionary<string, IDataType>(); 
		public IDataType GetTypeByGenericAlias(string alias)
		{
			IDataType dt;
			if (!_genericAliasToType.TryGetValue(alias, out dt))
				_genericAliasToType[alias] = dt = Cache(_baseCompiler.GetTypeByGenericAlias(alias));
			return dt;
		}

		readonly Dictionary<string, IDataType> _valueBindingAliasToType = new Dictionary<string, IDataType>(); 
		public IDataType GetTypeByValueBindingAlias(string alias)
		{
			IDataType dt;
			if (!_valueBindingAliasToType.TryGetValue(alias, out dt))
				_valueBindingAliasToType[alias] = dt = Cache(_baseCompiler.GetTypeByValueBindingAlias(alias));
			return dt;
		}

		IDataType Cache(IDataType dt)
		{
			if (dt == null) return null;
			return new CachingDataType(dt);
		}

		public IDataType GetAttachedPropertyTypeByName(string name)
		{
			return _baseCompiler.GetAttachedPropertyTypeByName(name);
		}

		public IEnumerable<IDataType> DataTypes
		{
			get { return _dataTypes; }
		}
	}
}