using Outracks.CodeCompletion.UXNinja.PerformanceTests.Core.Runner;
using Uno.UXNinja.PerformanceTests.Client.BasicTypes;

namespace Uno.UXNinja.PerformanceTests.Client
{
    class Program
    {
        static void Main(string[] args)
        {
            var options = CommandLineOptions.From(args);
            new PerformanceTestRunner().DiscoverAndRun(GetFromCommandLineOptions(options));
        }

        static RunnerOptions GetFromCommandLineOptions(CommandLineOptions options)
        {
            return new RunnerOptions()
            {
                Paths = options.Paths,
                LogDirectoryName = options.LogDirectoryName,
                TestTimeout = options.TestTimeout,
                BuildNumber = options.BuildNumber,
                BranchName = options.BranchName,
                UnoLibraryPath = options.UnoLibraryPath
            };
        }
    }
}
