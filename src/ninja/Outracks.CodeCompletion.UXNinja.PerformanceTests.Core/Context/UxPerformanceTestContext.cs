using Uno.UXNinja.PerformanceTests.Core.Loggers;

namespace Outracks.CodeCompletion.UXNinja.PerformanceTests.Core.Context
{
    public class UxPerformanceTestContext : IContext
    {
        public UxPerformanceTestContext(IResultLogger currentLogger, string unoLibraryPath)
        {
            Logger = currentLogger;
            UnoLibraryPath = unoLibraryPath;
        }

        public IResultLogger Logger { get; set; }

        public string UnoLibraryPath { get; set; }
    }
}
