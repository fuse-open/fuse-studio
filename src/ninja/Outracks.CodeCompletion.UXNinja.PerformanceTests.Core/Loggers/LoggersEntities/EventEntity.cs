namespace Uno.UXNinja.PerformanceTests.Core.Loggers.LoggersEntities
{
    public class EventEntity
    {
        public EventEntity()
        { }

        public EventEntity(string name, string description, double durationInMs)
        {
            Name = name;
            DurationInMs = durationInMs;
            Description = description;
        }

        public string Name { get; set; }

        public string Description { get; set; }

        public double DurationInMs { get; set; }
    }
}
