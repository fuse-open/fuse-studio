using Uno;
using Uno.Compiler.ImportServices;

namespace Fuse
{
    public class HideAttribute: Attribute {}
    public class OptionalHideAttribute: Attribute {}

    public class InlineAttribute: Attribute {}

    public class ExtensionAttribute: Attribute {}
    public class ChildExtensionAttribute: Attribute {}

    public class DefaultInstanceAttribute: Attribute
    {
        public string TargetProperty;
        public string Type;
        public DefaultInstanceAttribute(string targetProperty, string type)
        {
            TargetProperty = targetProperty;
            Type = type;
        }
    }


    public class IconAttribute: Attribute
    {
        public string Path;
        public IconAttribute([Filename] string path)
        {
            Path = path;
        }
    }

    public class PriorityAttribute : Attribute
    {
        public int Priority;
        public PriorityAttribute(int Priority = 0)
        {
            this.Priority = Priority;
        }
    }

    public class GroupAttribute: Attribute
    {
        public string Name;
        public int Priority;
        public GroupAttribute(string name, int priority = 0) { Name = name; Priority = priority; }
    }

    public class HidesAttribute: Attribute
    {
        public string TargetProperty;
        public HidesAttribute(string targetProperty) { TargetProperty = targetProperty; }
    }

    public class AdvancedAttribute: Attribute {}

    public class SpawnableAttribute : Attribute { }

    public class SpawnsAttribute: Attribute
    {
        public string SourceType;
        public string TargetProperty;
        public SpawnsAttribute(string sourceType, string targetProperty)
        {
            SourceType = sourceType;
            TargetProperty = targetProperty;
        }
    }

    public class TransitionAttribute: Attribute
    {
        public string IncomingType;
        public string TargetType;
        public string TargetProperty;
        public TransitionAttribute(string incomingType, string targetType, string targetProperty)
        {
            IncomingType = incomingType;
            TargetType = targetType;
            TargetProperty = targetProperty;
        }
    }

    public class DragDropPriorityAttribute: Attribute {}

    public class DesignerNameAttribute: Attribute
    {
        public string Name;
        public DesignerNameAttribute(string name) { Name = name; }
    }

    public class ComponentOfAttribute: Attribute
    {
        public string EntityClass;
        public ComponentOfAttribute(string entityClass) { EntityClass = entityClass; }
    }

    public class DefaultComponentAttribute: Attribute
    {
        public string ComponentClass;
        public DefaultComponentAttribute(string componentClass) { ComponentClass = componentClass; }
    }

    public class RequiredComponentAttribute: Attribute
    {
        public string ComponentClass;
        public RequiredComponentAttribute(string componentClass) { ComponentClass = componentClass; }
    }

    public class ColorAttribute : Attribute {}
    public class ThicknessAttribute: Attribute {}

    public class RangeAttribute : Attribute
    {
        public float Min;
        public float Max;
        public float Exponent = 1;
        public RangeAttribute(float min, float max) { Min = min; Max = max; }
        public RangeAttribute(float min, float max, float exponent) { Min = min; Max = max; Exponent = exponent; }
    }

    public class IntervalAttribute : Attribute
    {
        public float Interval;
        public IntervalAttribute(float interval) { Interval = interval; }
    }

    public class RecursionSafeAttribute : Attribute {}
}
