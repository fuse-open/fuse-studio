using NUnit.Framework;
using SketchImporter.UxGenerator;

namespace SketchConverterTests
{
	[TestFixture]
	public class UxSizeTests
	{
		[Test]
		public void TestUxSizeEqualsUxSizeWithSameValuesReturnsTrue()
		{
			var s1 = new UxSize(3.2f, UxUnit.Percent);
			var s2 = new UxSize(3.2f, UxUnit.Percent);
			Assert.That(s1.Equals(s2), Is.True);
		}

		[Test]
		public void TestEqualReferenceVariableReturnsTrue()
		{
			var s1 = new UxSize(3.2f, UxUnit.Percent);
			var s2 = s1;
			Assert.That(s1.Equals(s2), Is.True);
		}

		[Test]
		public void TestEqualDifferentUxSizeReturnsFalse()
		{
			var s1 = new UxSize(3.2f, UxUnit.Percent);
			Assert.That(s1.Equals(new UxSize(3.2f, UxUnit.Points)), Is.False);
		}

		[Test]
		public void TestEqualOtherTypeReturnsFalse()
		{
			var s1 = new UxSize(3.2f, UxUnit.Percent);
			Assert.That(s1.Equals(new object()), Is.False);
		}

		[Test]
		public void TestEqualNullReturnsFalse()
		{
			var s1 = new UxSize(3.2f, UxUnit.Percent);
			Assert.That(s1.Equals(null), Is.False);
		}

		[Test]
		public void TestGetHashCode()
		{
			var s1 = new UxSize(3.2f, UxUnit.Percent);
			var s2 = new UxSize(3.2f, UxUnit.Pixels);
			var s3 = new UxSize(3.2f, UxUnit.Percent);


			Assert.That(s1.GetHashCode(), Is.Not.EqualTo(s2.GetHashCode()));
			Assert.That(s1.GetHashCode(), Is.EqualTo(s3.GetHashCode()));
		}
	}
}
