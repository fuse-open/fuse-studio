using System;

namespace Outracks.CodeNinja.Tests.Loggers
{
    public class TeamCityLogger : ILogger
    {
        public void TestStarted(int count, string displayName)
        {
            Write("testStarted name='{0}'", displayName);
        }

        public void TestPassed(string displayName)
        {
            Write("testFinished name='{0}'", displayName);
        }

        public void TestFailed(string displayName, Exception e)
        {
            Write("testFailed name='{0}' details='{1}'", displayName, e.Message);
        }

        public void LogMessage(string message)
        {
            Write(message);
        }

        private static void Write(string formatString, params object[] objects)
        {
            Write(String.Format(formatString, objects));
        }

        private static void Write(string message)
        {
            Console.WriteLine("##teamcity[" + message + "]");
        }
    }
}
