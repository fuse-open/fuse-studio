using System;

namespace Outracks.CodeNinja.Tests.Loggers
{
    public interface ILogger
    {
        void TestStarted(int count, string displayName);
        void TestPassed(string displayName);
        void TestFailed(string displayName, Exception e);
        void LogMessage(string message);
    }
}
