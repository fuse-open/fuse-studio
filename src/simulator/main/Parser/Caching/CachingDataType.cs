using System.Collections.Generic;
using Uno.UX;
using Uno.UX.Markup.Reflection;

namespace Outracks.Simulator.Parser
{
	class CachingDataType : IDataType
	{
		readonly IDataType _dataType;
		public CachingDataType(IDataType dataType)
		{
			_dataType = dataType;
		}

		readonly Dictionary<string, bool> _implements = new Dictionary<string, bool>();
		public bool Implements(IDataType interfaceType)
		{
			var name = interfaceType.FullName;
			bool ret;
			if (!_implements.TryGetValue(name, out ret))
				_implements[name] = ret = _dataType.Implements(
					interfaceType is CachingDataType
						? ((CachingDataType)interfaceType)._dataType
						: interfaceType);
			return ret;
		}

		string _qualifiedName;
		public string QualifiedName
		{
			get { return _qualifiedName ?? (_qualifiedName = _dataType.QualifiedName); }
		}

		string _fullName;
		public string FullName
		{
			get { return _fullName ?? (_fullName = _dataType.FullName); }
		}

		#region Forwarding to _dataType

		public bool IsInnerClass
		{
			get { return _dataType.IsInnerClass; }
		}

		public bool IsFreestanding
		{
			get { return _dataType.IsFreestanding; }
		}

		public IDataType ActualIDataTypeImpl
		{
			get { return _dataType.ActualIDataTypeImpl; }
		}

		public AutoGenericInfo AutoGenericInfo
		{
			get { return _dataType.AutoGenericInfo; }
		}

		public ValueBindingInfo ValueBindingInfo
		{
			get { return _dataType.ValueBindingInfo; }
		}

		public bool HasUXConstructor
		{
			get { return _dataType.HasUXConstructor; }
		}

		public bool IsValueType
		{
			get { return _dataType.IsValueType; }
		}

		public bool IsString
		{
			get { return _dataType.IsString; }
		}

		public string ImplicitPropertySetter
		{
			get { return _dataType.ImplicitPropertySetter; }
		}

		public IEnumerable<IProperty> Properties
		{
			get { return _dataType.Properties; }
		}

		public IEnumerable<IEvent> Events
		{
			get { return _dataType.Events; }
		}

		public IEnumerable<IGlobalResource> GlobalResources
		{
			get { return _dataType.GlobalResources; }
		}

		public ContentMode ContentMode
		{
			get { return _dataType.ContentMode; }
		}

		public bool IsGenericParametrization
		{
			get { return _dataType.IsGenericParametrization; }
		}
		public IEnumerable<string> Metadata
		{
			get { return _dataType.Metadata; }
		}

		public bool IsGlobalModule
		{
			get { return _dataType.IsGlobalModule; }
		}

		public string UXFunctionName
		{
			get
			{
				return _dataType.UXFunctionName;
			}
		}

		public string GetMissingPropertyHint(string propname)
		{
			return _dataType.GetMissingPropertyHint(propname);
		}

		public int GenericParameterCount
		{
			get
			{
				return _dataType.GenericParameterCount;
			}
		}

		public string UXUnaryOperatorName
		{
			get
			{
				return _dataType.UXUnaryOperatorName;
			}
		}

		public IDataType UXTestBootstrapper
		{
			get { return _dataType.UXTestBootstrapper; }
		}

		public IEnumerable<string> UXTestBootstrapperFor
		{
			get { return _dataType.UXTestBootstrapperFor; }
		}

		#endregion
	}
}
