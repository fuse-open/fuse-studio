using System.CodeDom;
using System.Linq;
using NUnit.Framework;
using SketchConverter.SketchModel;
using SketchConverter.SketchParser;

namespace SketchConverterTests
{
	[TestFixture]
	public class AttributedStringParserTests
	{
		[Test]
		public void ParseColoredTextSymbolVersion91()
		{
			// copy paste of _archive property on archevedAttributedString in TextSymbol from json-dump of TestProject.sketch
			// Sketch version 91
			var jsonobj = @"YnBsaXN0MDDUAQIDBAUGZ2hYJHZlcnNpb25YJG9iamVjdHNZJGFyY2hpdmVyVCR0b3ASAAGGoK8QGQcIDxAcHR4fJysyNT9CRUlNVVZXWFldYWNVJG51bGzTCQoLDA0OWE5TU3RyaW5nViRjbGFzc1xOU0F0dHJpYnV0ZXOAAoAYgANSQWHTERIKExcbV05TLmtleXNaTlMub2JqZWN0c6MUFRaABIAFgAajGBkagAeAC4APgBdXTlNDb2xvcl8QEE5TUGFyYWdyYXBoU3R5bGVfEB9NU0F0dHJpYnV0ZWRTdHJpbmdGb250QXR0cmlidXRl1CAhIgojJCUmVU5TUkdCXE5TQ29sb3JTcGFjZV8QEk5TQ3VzdG9tQ29sb3JTcGFjZUYxIDEgMQAQAYAIgArSKAopKlROU0lEEAGACdIsLS4vWiRjbGFzc25hbWVYJGNsYXNzZXNcTlNDb2xvclNwYWNlojAxXE5TQ29sb3JTcGFjZVhOU09iamVjdNIsLTM0V05TQ29sb3KiMzHVNjcKODk6Ozw9PVpOU1RhYlN0b3BzW05TQWxpZ25tZW50XE5TVGV4dEJsb2Nrc1tOU1RleHRMaXN0c4AAEAKADoAMgAzSEgpAQaCADdIsLUNEV05TQXJyYXmiQzHSLC1GR18QF05TTXV0YWJsZVBhcmFncmFwaFN0eWxlo0ZIMV8QEE5TUGFyYWdyYXBoU3R5bGXSCkpLTF8QGk5TRm9udERlc2NyaXB0b3JBdHRyaWJ1dGVzgBaAENMREgpOUVSiT1CAEYASolJTgBOAFIAVXxATTlNGb250U2l6ZUF0dHJpYnV0ZV8QE05TRm9udE5hbWVBdHRyaWJ1dGUjQDAAAAAAAABdSGVsdmV0aWNhTmV1ZdIsLVpbXxATTlNNdXRhYmxlRGljdGlvbmFyeaNaXDFcTlNEaWN0aW9uYXJ50iwtXl9fEBBOU0ZvbnREZXNjcmlwdG9yomAxXxAQTlNGb250RGVzY3JpcHRvctIsLVxiolwx0iwtZGVfEBJOU0F0dHJpYnV0ZWRTdHJpbmeiZjFfEBJOU0F0dHJpYnV0ZWRTdHJpbmdfEA9OU0tleWVkQXJjaGl2ZXLRaWpUcm9vdIABAAgAEQAaACMALQAyADcAUwBZAGAAaQBwAH0AfwCBAIMAhgCNAJUAoACkAKYAqACqAK4AsACyALQAtgC+ANEA8wD8AQIBDwEkASsBLQEvATEBNgE7AT0BPwFEAU8BWAFlAWgBdQF+AYMBiwGOAZkBpAGwAb0ByQHLAc0BzwHRAdMB2AHZAdsB4AHoAesB8AIKAg4CIQImAkMCRQJHAk4CUQJTAlUCWAJaAlwCXgJ0AooCkwKhAqYCvALAAs0C0gLlAugC+wMAAwMDCAMdAyADNQNHA0oDTwAAAAAAAAIBAAAAAAAAAGsAAAAAAAAAAAAAAAAAAANR";

			var logger = new MessageListLogger();
			var stringParser = new AttributedStringParser(logger);
			var sketchString = stringParser.Parse(jsonobj);

			Assert.That(sketchString, Is.Not.Null);
			Assert.That(sketchString.Attributes.Count, Is.EqualTo(1));
			Assert.That(sketchString.Attributes.First().Color.Red, Is.EqualTo(1));
			Assert.That(sketchString.Attributes.First().Color.Green, Is.EqualTo(1));
			Assert.That(sketchString.Attributes.First().Color.Blue, Is.EqualTo(1));
			Assert.That(sketchString.Attributes.First().Color.Alpha, Is.EqualTo(1));

			Assert.That(sketchString.Contents, Is.EqualTo("Aa"));
		}

		[Test]
		public void ParseColoredTextSymbolVersion93()
		{
			// copy paste of _archive property on archevedAttributedString in TextSymbol from json-dump of DefaultText.sketch
			// Sketch version 93
			var jsonobj = @"YnBsaXN0MDDUAQIDBAUGX2BYJHZlcnNpb25YJG9iamVjdHNZJGFyY2hpdmVyVCR0b3ASAAGGoK8QHAcIDxAcHR4fKywtLi8wMTIzOT1FRkdISUxQWFtVJG51bGzTCQoLDA0OWE5TU3RyaW5nViRjbGFzc1xOU0F0dHJpYnV0ZXOAAoAbgANbVGhlIGNvbnRlbnTTERIKExcbV05TLmtleXNaTlMub2JqZWN0c6MUFRaABIAFgAajGBkagAeAEYAZgBBfECpNU0F0dHJpYnV0ZWRTdHJpbmdDb2xvckRpY3Rpb25hcnlBdHRyaWJ1dGVfEB9NU0F0dHJpYnV0ZWRTdHJpbmdGb250QXR0cmlidXRlXxAQTlNQYXJhZ3JhcGhTdHlsZdMREgogJRukISIjJIAIgAmACoALpCYnKCmADIANgA6AD4AQU3JlZFVhbHBoYVRibHVlVWdyZWVuIz/nrQ5gAAAAIz/wAAAAAAAAIz/sIMZAAAAAIz+vTqY/////0jQ1NjdaJGNsYXNzbmFtZVgkY2xhc3Nlc1xOU0RpY3Rpb25hcnmiNjhYTlNPYmplY3TSCjo7PF8QGk5TRm9udERlc2NyaXB0b3JBdHRyaWJ1dGVzgBiAEtMREgo+QUSiP0CAE4AUokJDgBWAFoAXXxATTlNGb250U2l6ZUF0dHJpYnV0ZV8QE05TRm9udE5hbWVBdHRyaWJ1dGUjQCgAAAAAAABdSGVsdmV0aWNhTmV1ZdI0NUpLXxATTlNNdXRhYmxlRGljdGlvbmFyeaNKNjjSNDVNTl8QEE5TRm9udERlc2NyaXB0b3KiTzhfEBBOU0ZvbnREZXNjcmlwdG9y1FFSUwpUVVZXWk5TVGFiU3RvcHNbTlNBbGlnbm1lbnRfEB9OU0FsbG93c1RpZ2h0ZW5pbmdGb3JUcnVuY2F0aW9ugAAQBBABgBrSNDVZWl8QEE5TUGFyYWdyYXBoU3R5bGWiWTjSNDVcXV8QEk5TQXR0cmlidXRlZFN0cmluZ6JeOF8QEk5TQXR0cmlidXRlZFN0cmluZ18QD05TS2V5ZWRBcmNoaXZlctFhYlRyb290gAEACAARABoAIwAtADIANwBWAFwAYwBsAHMAgACCAIQAhgCSAJkAoQCsALAAsgC0ALYAugC8AL4AwADCAO8BEQEkASsBMAEyATQBNgE4AT0BPwFBAUMBRQFHAUsBUQFWAVwBZQFuAXcBgAGFAZABmQGmAakBsgG3AdQB1gHYAd8B4gHkAeYB6QHrAe0B7wIFAhsCJAIyAjcCTQJRAlYCaQJsAn8CiAKTAp8CwQLDAsUCxwLJAs4C4QLkAukC/gMBAxYDKAMrAzAAAAAAAAACAQAAAAAAAABjAAAAAAAAAAAAAAAAAAADMg==";

			var logger = new MessageListLogger();
			var stringParser = new AttributedStringParser(logger);
			var sketchString = stringParser.Parse(jsonobj);

			Assert.That(sketchString, Is.Not.Null);
			Assert.That(sketchString.Attributes.Count, Is.EqualTo(1));
			Assert.That(sketchString.Attributes.First().Color.Red, Is.EqualTo(0.73987501859664895));
			Assert.That(sketchString.Attributes.First().Color.Green, Is.EqualTo(0.061146922409534399));
			Assert.That(sketchString.Attributes.First().Color.Blue, Is.EqualTo(0.87900078296661399));
			Assert.That(sketchString.Attributes.First().Color.Alpha, Is.EqualTo(1));

			Assert.That(sketchString.Contents, Is.EqualTo("The content"));

		}
	}
}
