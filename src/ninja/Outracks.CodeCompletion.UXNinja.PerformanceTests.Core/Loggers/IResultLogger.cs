namespace Uno.UXNinja.PerformanceTests.Core.Loggers
{
    public interface IResultLogger
    {
        void LogTimeEvent(string name, string description, double seconds);

        void LogError(string message);

        void ProjectStarted(string projectName);

        void ProjectFinished();
    }
}
