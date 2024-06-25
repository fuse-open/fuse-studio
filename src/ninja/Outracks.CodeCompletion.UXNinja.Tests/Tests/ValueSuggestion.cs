using NUnit.Framework;

namespace Uno.UXNinja.Tests.Tests
{
	//[Ignore("This is not yet implemented for our text plugin API.")]
    public class ValueSuggestion : TestBase
    {
        [Test]
        public void Binding01()
        {
            AssertUX(
                @"
                <App>
                    <StackPanel>
                        <Panel ux:Binding=""$(Appearance)""
                </App>
                "
            );
        }

        [Test]
        public void Enum01()
        {
            AssertUX(
                @"
                <App>
                    <StackPanel Orientation=""$(Horizontal, Vertical)""
                </App>
                "
            );
        }

        [Test]
        public void Enum02()
        {
            AssertUX(
                @"
                <App>
                    <StackPanel Orientation=""$(Horizontal, Vertical, !Front)""
                "
            );
        }

        [Test]
        public void Bool01()
        {
            AssertUX(
               @"
                <App>
                    <Text IsMultiline=""$(true, false, !null)
                "
           );
        }
    }
}