namespace Uno.UXNinja.PerformanceTests.Core.Loggers
{
    public class ResultLoggersFactory
    {
        public static IResultLogger GetLogger(string outputDirectory, string branchName, string buildNumber)
        {
            if (outputDirectory.StartsWith("ftp://"))
            {
                return new ResultFtpLogger(outputDirectory, branchName, buildNumber);
            }
            return new ResultFileLogger(outputDirectory, branchName, buildNumber);
        }
    }
}
