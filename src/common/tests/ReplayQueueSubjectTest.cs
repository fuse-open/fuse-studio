using System;
using System.Collections.Generic;
using NUnit.Framework;

namespace Outracks.Tests
{
	class ReplayQueueSubjectTest
	{
		[Test]
		public void ReplayQueueSubject_Remembers()
		{
			var subject = new ReplayQueueSubject<string>();

			// While no-one is listening, a tree or two falls in the forest
			var firstTreeFalls = "A tree falls in the forest";
			subject.OnNext(firstTreeFalls);

			var secondTreeFalls = "Another tree falls in the forest";
			subject.OnNext(secondTreeFalls);

			// An observer arrives
			var firstList = new List<string>();
			var firstObserver = subject.Subscribe(firstList.Add);

			// The observer, even though it was not present, can see that two tree clearly fell while no one was looking, and consumes them
			Assert.That(firstList, Is.EquivalentTo(new[] { firstTreeFalls, secondTreeFalls }));

			// Another observer arrives
			var secondList = new List<string>();
			var secondObserver = subject.Subscribe(secondList.Add);

			// Since the trees that fell was consumed by the first observer, this observer doesn't notice anything
			Assert.That(secondList, Is.Empty);

			// A tree falls while both are looking
			var thirdTreeFalls = "A third tree falls in the forest";
			subject.OnNext(thirdTreeFalls);

			// Both of them consume the third tree, since they were looking at the same time
			Assert.That(firstList, Is.EquivalentTo(new[] { firstTreeFalls, secondTreeFalls, thirdTreeFalls }));
			Assert.That(secondList, Is.EquivalentTo(new [] { thirdTreeFalls }));

			// The observers leave
			firstObserver.Dispose();
			secondObserver.Dispose();

			var fouthTreeFalls = "A fourth tree falls in the forest";
			subject.OnNext(fouthTreeFalls);

			// The observers that have left should not in any way be affected
			Assert.That(firstList, Is.EquivalentTo(new[] { firstTreeFalls, secondTreeFalls, thirdTreeFalls }));
			Assert.That(secondList, Is.EquivalentTo(new[] { thirdTreeFalls }));

			// However, a third observer arriving would consume the one tree that fell after the other observers left
			var thirdList = new List<string>();
			var thirdObserver = subject.Subscribe(thirdList.Add);
			Assert.That(thirdList, Is.EquivalentTo(new[] { fouthTreeFalls }));

			// The first two observers should be unaffected by the new observer arriving too
			Assert.That(firstList, Is.EquivalentTo(new[] { firstTreeFalls, secondTreeFalls, thirdTreeFalls }));
			Assert.That(secondList, Is.EquivalentTo(new[] { thirdTreeFalls }));
		}
	}
}
