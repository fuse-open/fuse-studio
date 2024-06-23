using System;
using System.Collections.Generic;
using System.Linq;
using Uno.Compiler.API;
using Uno.Compiler.API.Domain;
using Uno.Compiler.API.Domain.IL;

namespace Outracks.UnoDevelop.UXNinja
{
	public class DataTypes
	{
		readonly ICompiler _compiler;
		readonly DataType []_accessibleUxTypes;

		public DataTypes(ICompiler compiler, Func<IEnumerable<DataType>, IEnumerable<DataType>> customUxClasses)
		{
			_compiler = compiler;

			var compiledTypes = _compiler.Utilities
				.FindAllTypes()
				.Where(IsDataTypeAccessible)
				.ToArray();

			_accessibleUxTypes = customUxClasses(compiledTypes)
				.Concat(compiledTypes)
				.ToArray();
		}

		public DataType []AccessibleUxTypes
		{
			get { return _accessibleUxTypes; }
		}

		public Namespace RootNamespace
		{
			get { return _compiler.Data.IL; }
		}

		static bool IsDataTypeAccessible(DataType dataType)
		{
			return dataType.Modifiers.HasFlag(Modifiers.Public) &&
				!dataType.IsStatic;
		}
	}
}