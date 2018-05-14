using System;
using Uno.UX.Markup.Reflection;

namespace Outracks.Simulator.Parser
{
	public class GhostProperty : IMutableProperty
	{
		readonly string _name;
		readonly IDataType _dataType;
		readonly IDataType _declaringType;

		public GhostProperty(string name, IDataType dataType, IDataType declaringType)
		{
			_name = name;
			_dataType = dataType;
			_declaringType = declaringType;
		}

		public bool Accepts(IDataType type)
		{
			return type.Implements(DataType);
		}

		public IDataType DeclaringType { get { return _declaringType; } }
		public string Name { get { return _name; } }
		public IDataType DataType { get { return _dataType; } }

		public bool CanGet { get { return true; } }
		public bool CanSet { get { return true; } }

		public bool IsUXOnlyAutoIfClassProperty { get { return false; } }
		public bool IsUXAutoNameTableProperty { get { return false;  } }
		public bool IsUXAutoClassNameProperty { get { return false; } }

		public bool IsUXNameProperty { get { return false; } }
		public bool IsUXFileNameProperty { get { return false; } }
		public bool IsConstructorArgument { get { return false; } }
		public bool IsStyleList { get { return false; } }
		public bool IsActualDataTypeAvailable { get { return true; } }
		public bool IsOfGenericArgumentType { get { return false; } }

		public IDataType ListItemType
		{
			get { throw new InvalidOperationException(); }
		}

		public PropertyType PropertyType
		{
			get
			{
				if (DataType.IsValueType || DataType.IsString) return PropertyType.Atomic;
				else return PropertyType.Atomic;
			}
		}

		public AutoBindingType AutoBindingType
		{
			get { return AutoBindingType.None; }
		}

		public string StyleListAddMethod
		{
			get { throw new InvalidOperationException(); }
		}

		public string OriginSetterName
		{
			get { throw new InvalidOperationException(); }
		}

		public string ValueChangedEvent
		{
			get { throw new InvalidOperationException(); }
		}

		public int UXArgumentIndex
		{
			get
			{
				return 0;
			}
		}

		public bool IsUXVerbatim
		{
			get { return false; }
		}

        public string UXAuxNameTable
        {
            get
            {
                return null;
            }
        }

        public IdentifierScope UXIdentifierScope
        {
            get
            {
                return IdentifierScope.Globals;
            }
        }
    }
}