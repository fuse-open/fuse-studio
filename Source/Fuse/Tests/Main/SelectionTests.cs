using NUnit.Framework;

namespace Outracks.Fuse.Tests
{
	using Protocol.Messages;

	[TestFixture]
	class SelectionTests
	{
		[Test]
		public void FindSelectionIndexWhenJavaScriptTagHasNoContent()
		{
			const string xml = "<App><JavaScript>function someFunction() {};</JavaScript><ClientPanel><DockPanel Height=\"56\" Dock=\"Top\" Background=\"#ffc\"><Text Background=\"#dfd\" Value=\"Some text\" Alignment=\"Center\" Color=\"#000\" FontSize=\"20\" ></Text></DockPanel></ClientPanel></App>";
			
			Assert.AreEqual(2, ExternalSelection.TryGetElementIndex(xml, new TextPosition { Line = 1, Character = xml.IndexOf("ClientPanel") }));
			Assert.AreEqual(3, ExternalSelection.TryGetElementIndex(xml, new TextPosition { Line = 1, Character = xml.IndexOf("DockPanel") }));
			Assert.AreEqual(4, ExternalSelection.TryGetElementIndex(xml, new TextPosition { Line = 1, Character = xml.IndexOf("Text") }));
		}
		
		[Test]
		public void FindSelectionIndexWhenJavaScriptTagHasNoContentEmptyText()
		{

			const string xml = "<App><JavaScript>function someFunction() {};</JavaScript><ClientPanel><DockPanel Height=\"56\" Dock=\"Top\" Background=\"#ffc\"><Text Background=\"#dfd\" Value=\"Some text\" Alignment=\"Center\" Color=\"#000\" FontSize=\"20\" /></DockPanel></ClientPanel></App>";

			Assert.AreEqual(2, ExternalSelection.TryGetElementIndex(xml, new TextPosition { Line = 1, Character = xml.IndexOf("ClientPanel") }));
			Assert.AreEqual(3, ExternalSelection.TryGetElementIndex(xml, new TextPosition { Line = 1, Character = xml.IndexOf("DockPanel") }));
			Assert.AreEqual(4, ExternalSelection.TryGetElementIndex(xml, new TextPosition { Line = 1, Character = xml.IndexOf("Text") }));
		}

		[Test]
		public void FindCorrectSelectionIndexWhenJavaScriptHasXmlSymbols()
		{
			const string xml = "<App><JavaScript>function someFunction() {for (var i = 0; i < 2; i++) {}}</JavaScript><ClientPanel><DockPanel Height=\"56\" Dock=\"Top\" Background=\"#ffc\"><Text Background=\"#dfd\" Value=\"Some text\" Alignment=\"Center\" Color=\"#000\" FontSize=\"20\" ></Text></DockPanel></ClientPanel></App>"; ;
			Assert.AreEqual(2, ExternalSelection.TryGetElementIndex(xml, new TextPosition { Line = 1, Character = xml.IndexOf("ClientPanel") }));
			Assert.AreEqual(3, ExternalSelection.TryGetElementIndex(xml, new TextPosition { Line = 1, Character = xml.IndexOf("DockPanel") }));
			Assert.AreEqual(4, ExternalSelection.TryGetElementIndex(xml, new TextPosition { Line = 1, Character = xml.IndexOf("Text") }));
		}

		[Test]
		public void FindSelectionIndexForXmlWithJavaScriptEmptyTextTag()
		{
			const string xml =
			"<App><JavaScript>function someFunction() {for (var i = 0; i < 2; i++) {}}</JavaScript><ClientPanel><DockPanel Height=\"56\" Dock=\"Top\" Background=\"#ffc\"><Text Background=\"#dfd\" Value=\"Some text\" Alignment=\"Center\" Color=\"#000\" FontSize=\"20\" /></DockPanel></ClientPanel></App>";
			Assert.AreEqual(2, ExternalSelection.TryGetElementIndex(xml, new TextPosition { Line = 1, Character = xml.IndexOf("ClientPanel") }));
			Assert.AreEqual(3, ExternalSelection.TryGetElementIndex(xml, new TextPosition { Line = 1, Character = xml.IndexOf("DockPanel") }));
			Assert.AreEqual(4, ExternalSelection.TryGetElementIndex(xml, new TextPosition { Line = 1, Character = xml.IndexOf("Text") }));
		}
	
		[Test]
		public void FindCorrectSelectionIndexWhenJavaScriptTagImportsFile()
		{
			const string xml =
				"<App><JavaScript File=\"dummy.js\"/><ClientPanel><JavaScript><remove-this-garbage></JavaScript><DockPanel><Text></Text></DockPanel></ClientPanel></App>";
			Assert.AreEqual(4, ExternalSelection.TryGetElementIndex(xml, new TextPosition { Line = 1, Character = xml.IndexOf("DockPanel") }));
		}
	}
}
