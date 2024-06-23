using System;
using System.Collections.Generic;
using Mono.Options;

namespace Uno.UXNinja.PerformanceTests.Client.BasicTypes
{
    public class CommandLineOptions
    {
        public List<string> Paths;
        public string LogDirectoryName;
        public TimeSpan TestTimeout;
        public string BuildNumber;
        public string BranchName;
        public string UnoLibraryPath;

        public static CommandLineOptions From(string[] args)
        {
            var help = false;
            var commandOptions = new CommandLineOptions
            {
                TestTimeout = TimeSpan.FromMinutes(2),
                UnoLibraryPath = "..\\..\\..\\Outracks.CodeCompletion.UXNinja.TestsCommon\\TestData\\Outracks.UXNinja.Tests.Library.unoproj"
            };

            var p = new OptionSet
            {
                { "h|?|help", "Show help", v => help = v != null},
                { "l|logdirectory=", "Output directory", v => commandOptions.LogDirectoryName=v },
                { "o|timeout=", "Timeout for individual tests (in seconds)", (int v) => { commandOptions.TestTimeout = TimeSpan.FromSeconds(v); }},
                { "b|branch-name=", "Branch name.", v => commandOptions.BranchName=v },
                { "unolib-path=", "Uno library path", v => commandOptions.UnoLibraryPath=v },
                { "build-number=", "Team City build number", v => commandOptions.BuildNumber=v },
            };

            try
            {
                var extra = p.Parse(args);
                commandOptions.Paths = extra;
            }
            catch (OptionException e)
            {
                Console.WriteLine(e);
                PrintHelpAndExit(p);
            }
            if (help)
            {
                PrintHelpAndExit(p);
            }

            if (string.IsNullOrEmpty(commandOptions.LogDirectoryName))
            {
                throw new ArgumentException("Output directory is required");
            }

            return commandOptions;
        }

        private static void PrintHelpAndExit(OptionSet p)
        {
            Console.WriteLine("USAGE: performancetest [OPTIONS] [PATHS TO SEARCH]");
            Console.WriteLine();
            Console.WriteLine("EXAMPLES:");
            Console.WriteLine("performancetest -logdirectory=PerformanceLogResuts");
            Console.WriteLine(@"performancetest Path\Projects -logdirectory=PerformanceLogResuts");
            Console.WriteLine(@"performancetest Path\Projects\Foo.unoproj Path\Projects\Bar.unoproj -logdirectory=PerformanceLogResuts");
            Console.WriteLine(@"performancetest Path\Projects Path\OtherProjects\Foo.unoproj -logdirectory=PerformanceLogResuts");
            Console.WriteLine();
            Console.WriteLine("OPTIONS");
            p.WriteOptionDescriptions(Console.Out);
            Environment.Exit(-1);
        }
    }
}
