using NUnit.Framework;

namespace Uno.UXNinja.Tests.Tests
{
	public class StartTagName : TestBase
	{
		[Test]
		public void Empty()
		{
			AssertUX(
				@"$(!App, !Panel, !Text, !Foo)
				"
			);
		}

		[Test]
		public void StartTagName01()
		{
			AssertUX(
				@"
				<$(App, Panel, Text, !Foo)
				"
			);
		}

		[Test]
		public void StartTagName02()
		{
			AssertUX(
				@"
				<App>
					<$(Panel, Text, !App)
				</App>
				"
			);
		}

		[Test]
		public void StartTagName03()
		{
			AssertUX(
				@"
				<App>
					<Panel>
						<Text>
							<$(!Text, !Panel)
						</Text>
					</Panel>
				</App>
				"
			);
		}

		[Test]
		public void StartTagName04()
		{
			AssertUX(
				@"
				<App>
					<Panel>
						<$(Panel, Text, !Button)
					</Panel>
				</App>
				"
			);
		}

		[Test]
		public void StartTagName05()
		{
			AssertUX(
				@"
				<App xmlns:usp=""Custom.Controls"">
					<Panel>
						<usp:$(Button, !Text, !Panel)
					</Panel>
				</App>
				"
			);
		}

		[Test]
		public void StartTagName06()
		{
			AssertUX(
				@"
				<App>
					<$(Text)
				</App>
				"
			);
		}

		[Test]
		public void StartTagName07()
		{
			AssertUX(
				@"
				<App>
					<Fuse.$(Text)
				</App>
				"
			);
		}

		[Test]
		public void StartTagName_UXClass0()
		{
			AssertUX(
				@"
				<App>
					<Panel ux:Class='Foo' />
					<$(Text, Foo)
				</App>
				"
			);
		}


		[Test]
		public void StartTagName_UXClass1()
		{
			AssertUX(
				@"
				<App>
					<Panel ux:Class='Foo'>
						<Text ux:Class='Bar' />
					</Panel>
					<$(Text, Foo, Bar)
				</App>
				"
			);
		}

		[Test]
		public void StartTagName_Namespaces()
		{
			AssertUX(
				@"
				<App>
					<$(Custom) />
					<Custom.$(Controls,!Text,!Panel) />
					<Custom.Controls.$(Button,!Text,!Panel) />
				</App>
				"
			);
		}

		[Test]
		public void StartTagName_StaticClass()
		{
			AssertUX(
				@"
				<$(NotStaticClass,!StaticClass)
				"
			);
		}

		[Test]
		public void StartTagName_CustomNSScope()
		{
			AssertUX(
				@"
				<Custom.Controls.Button xmlns:foo='Fuse'>
					<foo:$(Text)
				"
			);
		}
	}
}
