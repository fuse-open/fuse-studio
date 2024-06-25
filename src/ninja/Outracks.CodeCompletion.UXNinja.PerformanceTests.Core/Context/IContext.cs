using Uno.UXNinja.PerformanceTests.Core.Loggers;

namespace Outracks.CodeCompletion.UXNinja.PerformanceTests.Core.Context
{
    public interface IContext
    {
        IResultLogger Logger { get; set; }

        string UnoLibraryPath { get; set; }
    }
}
