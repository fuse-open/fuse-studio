using System;

namespace Uno.UXNinja.PerformanceTests.Core.Attributes
{
    [AttributeUsage(AttributeTargets.Method)]
    public class PerformanceTestAttribute : Attribute
    {
        public string Description { get; set; }

        public PerformanceTestAttribute()
        { }

        public PerformanceTestAttribute(string description)
        {
            Description = description;
        }
    }
}
