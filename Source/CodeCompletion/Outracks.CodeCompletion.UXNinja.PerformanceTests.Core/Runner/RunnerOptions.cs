using System;
using System.Collections.Generic;

namespace Outracks.CodeCompletion.UXNinja.PerformanceTests.Core.Runner
{
    public class RunnerOptions
    {
        public List<string> Paths { get; set; }
        public string LogDirectoryName { get; set; }
        public TimeSpan TestTimeout { get; set; }
        public string BuildNumber { get; set; }
        public string BranchName { get; set; }
        public string UnoLibraryPath { get; set; }
    }
}
