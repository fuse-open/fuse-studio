using System.Collections.Generic;
using System.Linq;
using System.Reactive.Linq;
using System.Threading.Tasks;
using NUnit.Framework;

namespace Outracks.Tests.Extensions.LiftedOperators
{
	[TestFixture]
	public class CacheExtensionTests
	{
		[Test]
		public async Task CachePerElement_calls_valueRemoved_delegate_when_value_has_been_removed()
		{
			var valueRemoved = Optional.None<char>();
			var finalSequence =
				await Observable.Return<IEnumerable<char>>("ABC")
					.Concat(Observable.Return("AC"))
					.CachePerElement(c => c, valueRemoved: c => valueRemoved = c)
					.Select(x => new string(x.ToArray()))
					.LastOrDefaultAsync();

			Assert.That(finalSequence, Is.EqualTo("AC"));
			Assert.That(valueRemoved.HasValue, Is.True);
			Assert.That(valueRemoved.Value, Is.EqualTo('B'));
		}

		[Test]
		public async Task CachePerElement_returns_elements_in_same_order_as_source()
		{
			var finalSequence =
				await Observable.Return<IEnumerable<char>>("XYZ")
					.Concat(Observable.Return("ZYXCBA"))
					.CachePerElement(x => x)
					.Select(x => new string(x.ToArray()))
					.LastOrDefaultAsync();

			Assert.That(finalSequence, Is.EqualTo("ZYXCBA"));
		}

		[Test]
		public async Task CachePerElement_using_overload_that_takes_keySelector_returns_cached_values()
		{
			var finalSequence =
				await Observable.Return<IEnumerable<char>>("ABCDEFG")
					.Concat(Observable.Return("abcdefg"))
					.CachePerElement(c => c, c => char.ToUpper(c))
					.Select(x => new string(x.ToArray()))
					.LastOrDefaultAsync();

			Assert.That(finalSequence, Is.EqualTo("ABCDEFG"));
		}
	}
}