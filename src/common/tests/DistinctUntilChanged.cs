using System.Linq;
using System.Reactive.Linq;
using NUnit.Framework;

namespace Outracks.Tests
{
	class DistinctUntilChangedTest
	{
		[Test]
		public void RearrangeItemsInSet()
		{
			var a = new[] { 1, 2, 3 };
			var a2 = new[] { 1, 2, 3 };
			var b = new[] { 1, 3, 2 };
			var src = new[] { a, a2, b }.ToObservable();

			var result = src.DistinctUntilSetChanged().ToEnumerable().ToArray();

			Assert.That(result.Length, Is.EqualTo(1));
			Assert.That(result[0], Is.EquivalentTo(a));
		}

		[Test]
		public void RearrangeItemsInSequence()
		{
			var a = new[] { 1, 2, 3 };
			var a2 = new[] { 1, 2, 3 };
			var b = new[] { 1, 3, 2 };
			var src = new[] { a, a2, b }.ToObservable();

			var result = src.DistinctUntilSequenceChanged().ToEnumerable().ToArray();

			Assert.That(result.Length, Is.EqualTo(2));
			Assert.That(result[0], Is.EquivalentTo(a));
			Assert.That(result[1], Is.EquivalentTo(b));
		}
	}
}
