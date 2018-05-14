using System.Collections.Generic;
using System.Linq;
using Uno.Compiler.API.Domain.IL;
using Uno.Compiler.API.Domain.IL.Members;
using Uno.UX;
using Property = Uno.Compiler.API.Domain.IL.Members.Property;

namespace Outracks.UnoDevelop.UXNinja
{
	public static class PropertyHelper
	{
		public static bool IsMemberAListAndDataTypeCompatibleWithIt(DataType dataType, DataType member)
		{
			return IsList(member) && member.GenericArguments != null && member.GenericArguments.Any(dataType.IsCompatibleWith);
		}

		static bool IsList(DataType dataType)
		{
			if (dataType.Interfaces != null)
			{
				if (dataType.Interfaces.Any(IsList))
					return true;
			}

			return dataType.MasterDefinition.QualifiedName == "Uno.Collections.IList";
		}

		public static IEnumerable<Property> GetAllComponentPrimaryOrContentProperties(DataType dataType)
		{
			return GetAllWriteableProperties(dataType).Where(p => p.Attributes.Any(a => IsAttributeComponentPrimaryOrContent(a.ReturnType)));
		}

		public static IEnumerable<Property> GetAllWriteableProperties(DataType dataType)
		{
			if(dataType.Base != null)
				foreach (var prop in GetAllWriteableProperties(dataType.Base))
					yield return prop;

			foreach (var prop in dataType.Properties.Where(IsPropertyWriteable))
				yield return prop;
		}

		public static IEnumerable<Parameter> GetAllConstructorArguments(DataType dt)
		{
			foreach (var constructor in dt.Constructors.Where(c => c.Attributes.Any(a => a.ReturnType.IsOfType(typeof(UXConstructorAttribute)))))
			{
				foreach (var arg in constructor.Parameters)
				{
					yield return arg;
				}
			}
		}

		static bool IsAttributeComponentPrimaryOrContent(DataType attribute)
		{
			return attribute.IsOfType(typeof(UXComponentsAttribute)) || attribute.IsOfType(typeof(UXPrimaryAttribute)) || attribute.IsOfType(typeof(UXContentAttribute));
		}

		static bool IsPropertyWriteable(Property property)
		{		
			return property.IsPublic &&
					((property.GetMethod != null && property.SetMethod != null && property.SetMethod.IsPublic) ||
					 IsList(property.ReturnType) ||
					 property.GetMethod == null);
		}
	}
}