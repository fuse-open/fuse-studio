using System;
using System.Collections.Immutable;
using System.Linq;
using System.Reactive;
using System.Reactive.Linq;
using System.Reactive.Subjects;
using System.Threading.Tasks;
using System.Xml.Linq;
using NUnit.Framework;
using Outracks.Fuse.Live;
using Outracks.IO;
using Outracks.Simulator;

namespace Outracks.Fuse.Markup
{
	[TestFixture]
	class ElementTest
	{
		[Test]
		public static async Task Replace()
		{
			var element = CreateTree("<Parent><FirstChild><GrandChild /></FirstChild><SecondChild /></Parent>");
			var children = ImmutableList.ToImmutableList((await element.Children.FirstAsync()));
			var child = children.First();
			var copy = await child.Copy();
			var newChildContent = SourceFragment.FromString("<ChangedChild><InnerChild /></ChangedChild>");
			await child.Replace(old =>
			{
				Assert.That(old, Is.EqualTo(copy));
				return Task.FromResult(newChildContent);
			});

			var newCopy = await child.Copy();
			Assert.That(newCopy, Is.EqualTo(newChildContent));

			var newChildren = await element.Children.FirstAsync();
			Assert.That(newChildren, Is.EqualTo(children));
			Assert.That((await element.Copy()).ToString(), Is.EqualTo("<Parent><ChangedChild><InnerChild /></ChangedChild><SecondChild /></Parent>"));
		}

		[Test]
		public static async Task Cut()
		{
			var parent = CreateTree();
			var siblings = ImmutableList.ToImmutableList((await parent.Children.FirstAsync()));
			var copy = await siblings[0].Copy();
			var cut = await siblings[0].Cut();
			Assert.That(cut, Is.EqualTo(copy));

			var newSiblings = await parent.Children.FirstAsync();
			Assert.That(newSiblings, Is.EquivalentTo(siblings.RemoveAt(0)));
		}

		[Test]
		public static async Task Copy()
		{
			var element = CreateTree();
			var src = await element.Copy();
			Assert.That(src.ToString(), Is.EqualTo("<Parent><FirstChild /><SecondChild /></Parent>"));
		}

		[Test]
		public static async Task Paste()
		{
			var parent = CreateTree();
			var siblings = ImmutableList.ToImmutableList((await parent.Children.FirstAsync()));
			var childFragment = SourceFragment.FromString("<TheThing/>");
			var child = parent.Paste(childFragment);

			var newSiblings = await parent.Children.FirstAsync();
			Assert.That(newSiblings, Is.EqualTo(siblings.Add(child)));

			var src = await parent.Copy();
			Assert.That(src.ToString(), Is.EqualTo("<Parent><FirstChild /><SecondChild /><TheThing /></Parent>"));
		}

		[Test]
		[TestCase(
			"<App>\n    <Parent>\n        <Removed />\n    </Parent>\n</App>",
			"<App>\n    <Parent />\n</App>")]
		[TestCase(
			"<App>\n    <Parent>\n        <Keep />\n        <Removed />\n        <Keep />\n    </Parent>\n</App>",
			"<App>\n    <Parent>\n        <Keep />\n        <Keep />\n    </Parent>\n</App>")]
		[TestCase(
			"<App>\n    <Parent>\n        <Removed />\n        <Keep />\n    </Parent>\n</App>",
			"<App>\n    <Parent>\n        <Keep />\n    </Parent>\n</App>")]
		[TestCase(
			"<App>\n    <Parent>\n        <Keep />\n        <Removed />\n    </Parent>\n</App>",
			"<App>\n    <Parent>\n        <Keep />\n    </Parent>\n</App>")]
		public static async Task Cut_indents_surrounding_remaining_area(string initial, string expected)
		{
			Console.WriteLine("Initial:\n{0}\n", initial.Replace("\t", "⭾"));
			var root = (LiveElement) CreateTree();
			root.UpdateFrom(SourceFragment.FromString(initial).ToXml());
			var parent = (await root.Children.FirstAsync()).First();

			var removed = (await parent.Children.WherePerElement(x => x.Name.Is("Removed")).FirstAsync()).First();
			await removed.Cut();

			var src = await root.Copy();
			Console.WriteLine("Expected:\n{0}\n", expected.Replace("\t", "⭾"));
			var actual = src.ToString();
			Console.WriteLine("Actual:\n{0}\n", actual.Replace("\t", "⭾"));
			Assert.That(actual, Is.EqualTo(expected.Replace("\n", Environment.NewLine)));
		}

		[Test]
		[TestCase(
			"<App>\n\t<Parent>\n\t\t<ExistingChild />\n\t\t<Copied>\n\t\t\t<ThingChild />\n\t\t</Copied>\n\t</Parent>\n</App>",
			"<Copied>\n\t<ThingChild />\n</Copied>")]
		[TestCase(
			"<App>\n\t<Parent>\n\t\t<ExistingChild />\n\t\t<Copied>\n\t\t\t<ThingChild>\n\t\t\t\t<GrandChild />\n\t\t\t</ThingChild>\n\t\t</Copied>\n\t</Parent>\n</App>",
			"<Copied>\n\t<ThingChild>\n\t\t<GrandChild />\n\t</ThingChild>\n</Copied>")]
		public static async Task Copy_element_spanning_several_lines_removes_outer_indentation(string initial, string expected)
		{
			Console.WriteLine("Initial:\n{0}\n", initial.Replace("\t", "⭾"));
			var root = (LiveElement) CreateTree();
			root.UpdateFrom(SourceFragment.FromString(initial).ToXml());
			var parent = (await root.Children.FirstAsync()).First();
			var copied = (await parent.Children.WherePerElement(x => x.Name.Is("Copied")).FirstAsync()).First();
			var actual = (await copied.Copy()).ToString();
			Console.WriteLine("Expected:\n{0}\n", expected.Replace("\t", "⭾"));
			Console.WriteLine("Actual:\n{0}\n", actual.Replace("\t", "⭾"));
			Assert.That(actual, Is.EqualTo(expected.Replace("\n", Environment.NewLine)));
		}

		[Test]
		[TestCase(
			"<App>\n    <Parent/>\n</App>",
			"<TheThing />",
			"<App>\n    <Parent>\n        <TheThing />\n    </Parent>\n</App>")]
		[TestCase(
			"<App>\n\t<Parent/>\n</App>",
			"<TheThing />",
			"<App>\n\t<Parent>\n\t\t<TheThing />\n\t</Parent>\n</App>")]
		[TestCase(
			"<App>\n\t<Parent></Parent>\n</App>",
			"<TheThing />",
			"<App>\n\t<Parent>\n\t\t<TheThing />\n\t</Parent>\n</App>")]
		[TestCase(
			"<App>\n\t<Parent>\n\t\t<!-- COMMENT -->\n</Parent>\n</App>",
			"<TheThing />",
			"<App>\n\t<Parent>\n\t\t<!-- COMMENT -->\n\t\t<TheThing />\n</Parent>\n</App>")]
		[TestCase(
			"<App>\n\t<Parent>\n\t</Parent>\n</App>",
			"<TheThing />",
			"<App>\n\t<Parent>\n\t\t<TheThing />\n\t</Parent>\n</App>")]
		[TestCase(
			"<App>\n\t<Parent>\n\t\t<ExistingChild />\n\t</Parent>\n</App>",
			"<TheThing />",
			"<App>\n\t<Parent>\n\t\t<ExistingChild />\n\t\t<TheThing />\n\t</Parent>\n</App>")]
		[TestCase(
			"<App>\n\t<Parent>\n\t\t<ExistingChild />\n\t</Parent>\n</App>",
			"<TheThing>\n\t<ThingChild />\n</TheThing>",
			"<App>\n\t<Parent>\n\t\t<ExistingChild />\n\t\t<TheThing>\n\t\t\t<ThingChild />\n\t\t</TheThing>\n\t</Parent>\n</App>")]
		[TestCase(
			"<App>\n\t<Parent />\n</App>",
			"<TheThing>\n\t<ThingChild />\n</TheThing>",
			"<App>\n\t<Parent>\n\t\t<TheThing>\n\t\t\t<ThingChild />\n\t\t</TheThing>\n\t</Parent>\n</App>")]
		[TestCase(
			"<App>\n\t<Parent />\n</App>",
			"<TheThing>\n\t<ThingChild>\n\t\t<GrandChild />\n\t</ThingChild>\n</TheThing>",
			"<App>\n\t<Parent>\n\t\t<TheThing>\n\t\t\t<ThingChild>\n\t\t\t\t<GrandChild />\n\t\t\t</ThingChild>\n\t\t</TheThing>\n\t</Parent>\n</App>")]
		public static async Task Paste_indents_element(string initial, string pasted, string expected)
		{
			Console.WriteLine("Initial:\n{0}\n", initial.Replace("\t", "⭾"));
			var root = (LiveElement) CreateTree();
			root.UpdateFrom(SourceFragment.FromString(initial).ToXml());
			var parent = (await root.Children.FirstAsync()).First();

			var childFragment = SourceFragment.FromString(pasted);
			parent.Paste(childFragment);

			var src = await root.Copy();
			Console.WriteLine("Expected:\n{0}\n", expected.Replace("\t", "⭾"));
			var actual = src.ToString();
			Console.WriteLine("Actual:\n{0}\n", actual.Replace("\t", "⭾"));
			Assert.That(actual, Is.EqualTo(expected.Replace("\n", Environment.NewLine)));
		}

		[Test]
		[TestCase(
			"<App>\n</App>",
			"<TheThing />",
			"<App>\n\t<TheThing />\n</App>")]
		[TestCase(
			"<App />",
			"<TheThing />",
			"<App>\n\t<TheThing />\n</App>")]
		public static async Task Paste_in_root_indents_element_using_tab_by_default(string initial, string pasted, string expected)
		{
			Console.WriteLine("Initial:\n{0}\n", initial.Replace("\t", "⭾"));
			var root = (LiveElement)CreateTree();
			root.UpdateFrom(SourceFragment.FromString(initial).ToXml());

			var childFragment = SourceFragment.FromString(pasted);
			root.Paste(childFragment);

			var src = await root.Copy();
			Console.WriteLine("Expected:\n{0}\n", expected.Replace("\t", "⭾"));
			var actual = src.ToString();
			Console.WriteLine("Actual:\n{0}\n", actual.Replace("\t", "⭾"));
			Assert.That(actual, Is.EqualTo(expected.Replace("\n", Environment.NewLine)));
		}



		[Test]
		[TestCase(
			"<App>\n  <Sibling />\n</App>",
			"<TheThing />",
			"<App>\n  <Sibling />\n  <TheThing />\n</App>")]
		[TestCase(
			"<App>\n  <First />\n  <Sibling />\n</App>",
			"<TheThing />",
			"<App>\n  <First />\n  <Sibling />\n  <TheThing />\n</App>")]
		public static async Task Paste_after_indents_element(string initial, string pasted, string expected)
		{
			Console.WriteLine("Initial:\n{0}\n", initial.Replace("\t", "⭾"));
			var root = (LiveElement) CreateTree();
			root.UpdateFrom(SourceFragment.FromString(initial).ToXml());
			var parent = (await root.Children.WherePerElement(x => x.Name.Is("Sibling")).FirstAsync()).First();

			var pastedFragment = SourceFragment.FromString(pasted);
			parent.PasteAfter(pastedFragment);

			var src = await root.Copy();
			Console.WriteLine("Expected:\n{0}\n", expected.Replace("\t", "⭾"));
			var actual = src.ToString();
			Console.WriteLine("Actual:\n{0}\n", actual.Replace("\t", "⭾"));
			Assert.That(actual, Is.EqualTo(expected.Replace("\n", Environment.NewLine)));
		}

		[Test]
		[TestCase(
			"<App>\n  <Sibling />\n</App>",
			"<TheThing />",
			"<App>\n  <TheThing />\n  <Sibling />\n</App>")]
		[TestCase(
			"<App>\n  <First />\n  <Sibling />\n</App>",
			"<TheThing />",
			"<App>\n  <First />\n  <TheThing />\n  <Sibling />\n</App>")]
		public static async Task Paste_before_indents_element(string initial, string pasted, string expected)
		{
			Console.WriteLine("Initial:\n{0}\n", initial.Replace("\t", "⭾"));
			var root = (LiveElement) CreateTree();
			root.UpdateFrom(SourceFragment.FromString(initial).ToXml());
			var parent = (await root.Children.WherePerElement(x => x.Name.Is("Sibling")).FirstAsync()).First();

			var pastedFragment = SourceFragment.FromString(pasted);
			parent.PasteBefore(pastedFragment);

			var src = await root.Copy();
			Console.WriteLine("Expected:\n{0}\n", expected.Replace("\t", "⭾"));
			var actual = src.ToString();
			Console.WriteLine("Actual:\n{0}\n", actual.Replace("\t", "⭾"));
			Assert.That(actual, Is.EqualTo(expected.Replace("\n", Environment.NewLine)));
		}

		[Test]
		public static async Task PasteAfter()
		{
			var parent = CreateTree();
			var siblings = ImmutableList.ToImmutableList((await parent.Children.FirstAsync()));

			var childFragment = SourceFragment.FromString("<TheThing/>");
			var child = siblings[0].PasteAfter(childFragment);

			var newSiblings = await parent.Children.FirstAsync();
			Assert.That(newSiblings, Is.EqualTo(siblings.Insert(1, child)));

			var src = await parent.Copy();
			Assert.That(src.ToString(), Is.EqualTo("<Parent><FirstChild /><TheThing /><SecondChild /></Parent>"));
		}

		[Test]
		public static async Task PasteBefore()
		{
			var parent = CreateTree();
			var siblings = ImmutableList.ToImmutableList((await parent.Children.FirstAsync()));

			var childFragment = SourceFragment.FromString("<TheThing/>");
			var child = siblings[0].PasteBefore(childFragment);

			var newSiblings = await parent.Children.FirstAsync();
			Assert.That(newSiblings, Is.EqualTo(siblings.Insert(0, child)));

			var src = await parent.Copy();
			Assert.That(src.ToString(), Is.EqualTo("<Parent><TheThing /><FirstChild /><SecondChild /></Parent>"));
		}

		[Test]
		public static async Task UpdateFrom_gives_correct_children_ordering()
		{
			var parent = (LiveElement) CreateTree();
			parent.UpdateFrom(XElement.Parse(@"<App><Circle /><Text /></App>"));
			parent.UpdateFrom(XElement.Parse(@"<App><Text /><Circle /><Text /></App>"));
			var childElementNames =
				await parent.Children.Select(
						children => children.Select(el => (IObservable<string>) el.Name)
							.CombineLatest())
					.Switch()
					.FirstAsync();

			CollectionAssert.AreEqual(childElementNames, new [] { "Text", "Circle", "Text" });
		}

		static IElement CreateTree(string xmlText = "<Parent><FirstChild /><SecondChild /></Parent>")
		{
			var parent = new LiveElement(
				AbsoluteFilePath.Parse("foo/bar"),
				Observable.Never<ILookup<ObjectIdentifier, ObjectIdentifier>>(),
				Observable.Return(true),
				new Subject<Unit>(),
				Observer.Create<IBinaryMessage>(msg => { }),
				s => Element.Empty);

			parent.UpdateFrom(SourceFragment.FromString(xmlText).ToXml());

			return parent;
		}

	}
}
