using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using Uno.UX;
using Uno.UX.Markup.Reflection;

namespace Outracks.Simulator.Parser
{
	class GhostDataType : IDataType
	{
		readonly string _name;
		readonly IDataType _baseType;
		readonly Func<GhostDataType, IEnumerable<IGlobalResource>> _globalResourcesFactory;
		readonly Func<GhostDataType, IEnumerable<IProperty>> _declaredPropertiesFactory;

		ImmutableList<IGlobalResource> _globalResources;
		ImmutableList<IProperty> _declaredProperties;

		public GhostDataType(
			string name, 
			IDataType baseType, 
			Func<GhostDataType, IEnumerable<IGlobalResource>> globalResources,
			Func<GhostDataType, IEnumerable<IProperty>> declaredProperties)
		{
			if (string.IsNullOrWhiteSpace(name))
				throw new ArgumentNullException("name");
			if (baseType == null)
				throw new ArgumentNullException("baseType");

			_name = name;
			_baseType = baseType;
			_globalResourcesFactory = globalResources;
			_declaredPropertiesFactory = declaredProperties;
		}

		public void CompleteType()
		{
			if (_globalResources != null)
				throw new InvalidOperationException("Ghost type `"+_name+"` is already completed");

			_globalResources = _globalResourcesFactory(this).ToImmutableList();
			_declaredProperties = _declaredPropertiesFactory(this).ToImmutableList();
		}

		public bool IsInnerClass
		{
			get { return false; }
		}

		public bool IsFreestanding
		{
			get { return _baseType.IsFreestanding; }
		}

		public string QualifiedName
		{
			get { return _name; }
		}

		public string FullName
		{
			get { return _name; }
		}

		public IEnumerable<IGlobalResource> GlobalResources
		{
			get { return _globalResources.Union(_baseType.GlobalResources, new GlobalResourceEqualityComparer()); }
		}
		public IEnumerable<IProperty> Properties
		{
			get { return _baseType.Properties.Union(DeclaredUXProperties); }
		}

		public IEnumerable<IProperty> DeclaredUXProperties
		{
			get { return _declaredProperties; }
		}

		#region Forwarding to _baseType

		public bool Implements(IDataType interfaceType)
		{
			return _baseType.Implements(interfaceType);
		}

		public bool IsGenericParametrization
		{
			get { return _baseType.IsGenericParametrization; }
		}

		public IDataType ActualIDataTypeImpl
		{
			get { return _baseType.ActualIDataTypeImpl; }
		}

		public AutoGenericInfo AutoGenericInfo
		{
			get { return _baseType.AutoGenericInfo; }
		}

		public ValueBindingInfo ValueBindingInfo
		{
			get { return _baseType.ValueBindingInfo; }
		}

		public bool HasUXConstructor
		{
			get { return _baseType.HasUXConstructor; }
		}

		public bool IsValueType
		{
			get { return _baseType.IsValueType; }
		}

		public bool IsString
		{
			get { return _baseType.IsString; }
		}

		public string ImplicitPropertySetter
		{
			get { return _baseType.ImplicitPropertySetter; }
		}

		public IEnumerable<IEvent> Events
		{
			get { return _baseType.Events; }
		}

		public ContentMode ContentMode
		{
			get { return _baseType.ContentMode; }
		}

		public IEnumerable<string> Metadata
		{
			get { return _baseType.Metadata; }
		}

		public bool IsGlobalModule
		{
			get { return _baseType.IsGlobalModule; }
		}

		public string UXFunctionName
		{
			get
			{
				return _baseType.UXFunctionName;
			}
		}

		public string GetMissingPropertyHint(string propname)
		{
			return _baseType.GetMissingPropertyHint(propname);
		}

		public int GenericParameterCount
		{
			get
			{
				return _baseType.GenericParameterCount;
			}
		}

		public string UXUnaryOperatorName
		{
			get
			{
				return _baseType.UXUnaryOperatorName;
			}
		}

		public IDataType UXTestBootstrapper
		{
			get { return _baseType.UXTestBootstrapper; }
		}

		public IEnumerable<string> UXTestBootstrapperFor
		{
			get { return _baseType.UXTestBootstrapperFor; }
		}

		#endregion
	}

	class GlobalResourceEqualityComparer : IEqualityComparer<IGlobalResource>
	{
		public bool Equals(IGlobalResource x, IGlobalResource y)
		{
			return x.GlobalSymbol.Equals(y.GlobalSymbol);
		}

		public int GetHashCode(IGlobalResource obj)
		{
			return obj.GlobalSymbol.GetHashCode();
		}
	}
}
