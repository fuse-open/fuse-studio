using System;

namespace Uno.UXNinja.PerformanceTests.Core.Attributes
{
    public class IgnorePerformanceTestAttribute : Attribute
    {
        public string Reason { get; set; }

        public IgnorePerformanceTestAttribute()
        { }

        public IgnorePerformanceTestAttribute(string reason)
        {
            Reason = reason;
        }
    }
}
