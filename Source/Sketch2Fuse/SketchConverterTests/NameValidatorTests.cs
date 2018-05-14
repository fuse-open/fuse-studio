using NUnit.Framework;
using SketchConverter.UxBuilder;

namespace SketchConverterTests
{
	public class NameValidatorTests
	{
		[Test]
		public void NameIsValid_ValidNames([Values("a", "aAaa", "A1", "a_1", "_a", "_1")] string name)
		{
			Assert.That(NameValidator.NameIsValid(name), Is.EqualTo(NameValidity.Valid));
		}

		[Test]
		public void NameIsValid_InvalidCharacters([Values("1", "1A", "", "a B", "aå", "a-b", "a.b")] string name)
		{
			Assert.That(NameValidator.NameIsValid(name), Is.EqualTo(NameValidity.InvalidCharacter));
		}

		[Test]
		public void NameIsValid_UnoKeywords([Values("draw", "local", "add")] string name)
		{
			Assert.That(NameValidator.NameIsValid(name), Is.EqualTo(NameValidity.InvalidKeyword));
		}

		[Test]
		public void NameIsValid_CSharpKeywords([Values("async", "string", "class")] string name)
		{
			Assert.That(NameValidator.NameIsValid(name), Is.EqualTo(NameValidity.InvalidKeyword));
		}	}
}
