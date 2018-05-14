using Uno.UXNinja.PerformanceTests.Core;
using Uno.UXNinja.PerformanceTests.Core.Attributes;

namespace Uno.UXNinja.PerformanceTests.Tests
{
    public class SomeTest : TestBase
    {
        [PerformanceTest("Some description here")]
        public void Test01()
        {
            TestPerformance(
@"
    <App>
        <Text $
    </App>
"
                );
        }
    }
}
