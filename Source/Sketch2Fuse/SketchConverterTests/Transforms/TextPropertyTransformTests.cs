using System.Collections.Generic;
using System.Linq;
using NUnit.Framework;
using SketchConverter.Transforms;
using SketchImporter.UxGenerator;

namespace SketchConverterTests.Transforms
{
	public class TextPropertyTransformTests
	{

		[Test]
		public void CreatesPropertyForStringValue()
		{
			var log = new MessageListLogger();
			var transform = new TextPropertyTransform(log);

			var node = new UxNode("Panel");
			node.Children.Add(CreateTextNode("TextLayer", "Guten Tag!"));
			
			transform.Apply(node);
			
			var propertyShouldBeFirstElement = (UxNode)node.Children.ToList()[0];
			AssertHasAttribute(propertyShouldBeFirstElement, "ux:Property", "TextLayer");

			var texts = node.ChildrenOfClass("Text").ToList();
			Assert.That(texts.Count(), Is.EqualTo(1));
			AssertHasAttribute(texts[0], "Value", "{Property TextLayer}");
			
			AssertHasAttribute(node, "TextLayer", "Guten Tag!");
		}
		
		[Test]
		public void CreatesPropertiesForSeveralStringValues()
		{
			var log = new MessageListLogger();
			var transform = new TextPropertyTransform(log);

			var node = new UxNode("Panel");
			node.Children.Add(CreateTextNode("FirstLayer", "Guten Tag!"));
			node.Children.Add(CreateTextNode("OtherLayer", "Wie geht's?"));
			
			transform.Apply(node);
			
			var firstPropertyShouldBeFirstElement = (UxNode)node.Children.ToList()[0];
			AssertHasAttribute(firstPropertyShouldBeFirstElement, "ux:Property", "FirstLayer");
			
			var secondPropertyShouldBeSecondElement = (UxNode)node.Children.ToList()[1];
			AssertHasAttribute(secondPropertyShouldBeSecondElement, "ux:Property", "OtherLayer");

			var texts = node.ChildrenOfClass("Text").ToList();
			Assert.That(texts.Count(), Is.EqualTo(2));
			AssertHasAttribute(texts[0], "Value", "{Property FirstLayer}");
			AssertHasAttribute(texts[1], "Value", "{Property OtherLayer}");
			
			AssertHasAttribute(node, "FirstLayer", "Guten Tag!");
			AssertHasAttribute(node, "OtherLayer", "Wie geht's?");
		}

		[Test]
		public void SkipsAndLogsInvalidNames()
		{
			var log = new MessageListLogger();
			var transform = new TextPropertyTransform(log);

			var node = new UxNode("Panel");
			node.Children.Add(CreateTextNode("I have a space", "Ohayou Gozaimasu!"));
			node.Children.Add(CreateTextNode("var", "Konbanwa"));
			
			transform.Apply(node);
			
			var stringProperties = node.Children.Where(c => c is UxNode).Cast<UxNode>().Where(c => c.ClassName == "string");
			Assert.That(stringProperties.Count(), Is.EqualTo(0));
			
			Assert.That(node.Attributes.Keys, Does.Not.Contain("var"));
			Assert.That(node.Attributes.Keys, Does.Not.Contain("I have a space"));

			Assert.That(log.Warnings(), Does.Contain("Could not create a text property for the layer 'var', as 'var' is a reserved word. Please choose another name."));
			Assert.That(log.Warnings(), Does.Contain("Could not create a text property for the layer 'I have a space', as it contains an invalid character. Please only use the letters a-z, numbers, or underscores, and don't start the name with a number."));
		}

		[Test]
		public void GivenDuplicateNamesCreatesPropertyForOneAndLogsErrorForTheOther()
		{
			var log = new MessageListLogger();
			var transform = new TextPropertyTransform(log);

			var node = new UxNode("Panel");
			node.Children.Add(CreateTextNode("Strata", "Grata"));
			node.Children.Add(CreateTextNode("Strata", "NonGrata"));
			
			transform.Apply(node);

			AssertHasAttribute((UxNode)node.Children.ToList()[0], "ux:Property", "Strata");
			AssertHasAttribute(node.ChildrenOfClass("Text").ToList()[0], "Value", "{Property Strata}");
			AssertHasAttribute(node, "Strata", "Grata");

			Assert.That(log.Warnings().Count(), Is.EqualTo(1));
			Assert.That(log.Warnings().First(), Is.EqualTo("Could not create a text property for the layer 'Strata', as a text property for another layer with the same name has already been created. Please use unique names for text layers within the same symbol."));
		}

		private UxNode CreateTextNode(string layerName, string value)
		{
			var text = new UxNode("Text") {SketchLayerName = layerName};
			text.Attributes["Value"] = new UxString(value);
			return text;
		}

		private void AssertHasAttribute(UxNode node, string name, string value)
		{
			var attributes = node.Attributes;
			Assert.That(attributes.Keys, Does.Contain(name));
			Assert.That(((UxString)attributes[name]).Value, Is.EqualTo(value));
		}
	}

	public static class Extensions
	{
		public static IEnumerable<UxNode> ChildrenOfClass(this UxNode node, string className)
		{
			return node.Children
				.Where(c => c is UxNode && ((UxNode) c).ClassName == className)
				.Cast<UxNode>();
		}
	}
}
