using System;

namespace Outracks.CodeNinja.Tests.Loggers
{
    public class ConsoleLogger : ILogger
    {
        private readonly bool _onlyShowFailedTests;
        private ConsoleColor _originalColor = Console.ForegroundColor;

        public ConsoleLogger(bool onlyShowFailedTests)
        {
            _onlyShowFailedTests = onlyShowFailedTests;
        }
        public void TestStarted(int count, string displayName)
        {
            _originalColor = Console.ForegroundColor;
            Console.Write(count.ToString("000: ") + displayName + ": ");
        }

        public void TestPassed(string displayName)
        {
            if (_onlyShowFailedTests)
            {
                var cursorTop = Console.CursorTop;
                Console.SetCursorPosition(0, cursorTop);
                for (int i = 0; i < Console.WindowWidth; i++)
                    Console.Write(" ");
                Console.SetCursorPosition(0, cursorTop);
            }
            else
            {
                Console.ForegroundColor = ConsoleColor.Green;
                Console.WriteLine("PASSED");
            }
            Console.ForegroundColor = _originalColor;
        }

        public void TestFailed(string displayName, Exception e)
        {
            Console.ForegroundColor = ConsoleColor.Red;
            Console.Write("FAILED ");
            Console.ForegroundColor = ConsoleColor.DarkRed;
            Console.WriteLine("(" + e.Message + ")");
            Console.ForegroundColor = _originalColor;
        }

        public void LogMessage(string message)
        {
            Console.WriteLine(message);
        }
    }

}
