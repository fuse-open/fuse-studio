using NUnit.Framework;

namespace Uno.UXNinja.Tests.Tests
{
    public class StartAttributeName : TestBase
    {
        [Test]
        [Ignore("ux:Path isn't expected attribute for App tag https://github.com/fusetools/Fuse/issues/825")]
        public void StartAttributeName01()
        {
            AssertUX(
                @"
                <App $(ux:ClearColor, ux:Name, ux:ClassName, !ux:Path)>
                </App>
                "
            );
        }

        [Test]
        public void StartAttributeName02()
        {
            AssertUX(
                @"
                <App>
                    <Panel $(ux:Name, ux:ClassName, Width, Height, !Orientation, !ItemSpacing, !HasChildren)
                "
            );
        }

        [Test]
        public void StartAttributeName03()
        {
            AssertUX(
                @"
                <App>
                    <StackPanel $(Orientation, ItemSpacing)
                </App>
                "
            );
        }

        [Test]
        public void StartAttributeName04()
        {
            AssertUX(
                @"
                <App>
                    <Text $(IsMultiline, Content, !Orientation, !ItemSpacing, !ActualSize)
                </App>
                "
            );
        }

        [Test]
        public void StartAttributeName05()
        {
            AssertUX(
                @"
                <App xmlns:usp=""Custom.Controls"">
                    <usp:Button $(Text, Clicked)
                </App>
                "
            );
        }

		[Test]
		public void StartAttributeName_UXClass0()
		{
			AssertUX(
				@"
				<App>
					<Panel ux:Class='Foo'/>
					<Foo $(Width,Height)
				</App>
				"
			);
		}

		[Test]
		public void StartAttributeName_UXClass1()
		{
			AssertUX(
				@"
				<App>
					<Panel ux:Class='Foo'>
						<string ux:Property='Message' />
					</Panel>
					<Foo $(Width,Height,Message)
				</App>
				"
			);
		}

		[Test]
		public void StartAttributeName_UXClass2()
		{
			AssertUX(
				@"
				<App>
					<Panel ux:Class='Foo'>
						<string ux:Property='Message' />
					</Panel>
					<Foo ux:Class='Bar' />
					<Foo $(Width,Height,Message)
					<Bar $(Width,Height,Message)
				</App>
				"
			);
		}

		[Test]
		public void StartAttributeName_UXClass3()
		{
			AssertUX(
				@"
				<App>
					<Panel ux:Class='Foo'>
						<string ux:Property='Message' />
					</Panel>
					<Panel ux:Class='Bar'>
						<Foo ux:Property='Wut' />
					</Panel>
					<Foo $(Width,Height,Message) ux:Name='A' />
					<Bar $(Width,Height,Wut) />
				</App>
				"
			);
		}
	}
}