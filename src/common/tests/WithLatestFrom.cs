using System;
using System.Collections.Generic;
using System.Reactive;
using System.Reactive.Linq;
using System.Reactive.Subjects;
using NUnit.Framework;

namespace Outracks.Tests
{
	class WithLatestFrom
	{
		[Test]
		public void ColdStreamAndColdSampler()
		{
			var stream = Observable.Return("lick");
			var sampler = Observable.Return("tick");

			var results = new List<string>();
			var errors = new List<Exception>();
			var completed = new List<Unit>();
			stream.WithLatestFromBuffered(sampler, (a, b) => a).Subscribe(
				r =>
					results.Add(r),
				errors.Add,
				() => completed.Add(Unit.Default));

			Assert.AreEqual(1, results.Count, "OnNext");
			Assert.AreEqual(0, errors.Count, "OnError");
			Assert.AreEqual(1, completed.Count, "OnCompleted");

			Assert.AreEqual(results[0], "lick");
		}

		[Test]
		public void ColdStreamAndHotSampler()
		{
			var stream = Observable.Return("lick");
			var sampler = new Subject<Unit>();

			var results = new List<string>();
			var errors = new List<Exception>();
			var completed = new List<Unit>();
			stream.WithLatestFromBuffered(sampler, (a, b) => a).Subscribe(
				results.Add,
				errors.Add,
				() => completed.Add(Unit.Default));

			sampler.OnNext(Unit.Default);
			sampler.OnNext(Unit.Default);
			sampler.OnNext(Unit.Default);

			Assert.AreEqual(0, results.Count, "OnNext");
			Assert.AreEqual(0, errors.Count, "OnError");

			sampler.OnCompleted();

			Assert.AreEqual(1, completed.Count, "OnCompleted");
		}

		[Test]
		public void HotStreamAndHotSampler()
		{
			var stream = new Subject<string>();
			var sampler = new Subject<Unit>();

			var results = new List<string>();
			var errors = new List<Exception>();
			var completed = new List<Unit>();
			stream.WithLatestFromBuffered(sampler, (a, b) => a).Subscribe(
				results.Add,
				errors.Add,
				() => completed.Add(Unit.Default));

			stream.OnNext("dropped");
			stream.OnNext("emitted");

			sampler.OnNext(Unit.Default);
			sampler.OnNext(Unit.Default);
			sampler.OnNext(Unit.Default);

			Assert.AreEqual(1, results.Count, "OnNext");
			Assert.AreEqual(0, errors.Count, "OnError");

			stream.OnCompleted();

			Assert.AreEqual(1, completed.Count, "OnCompleted");

			Assert.AreEqual("emitted", results[0]);
		}

		[Test]
		public void HotStreamAndHotSampler2()
		{
			var stream = new Subject<string>();
			var sampler = new Subject<Unit>();

			var results = new List<string>();
			var errors = new List<Exception>();
			var completed = new List<Unit>();

			stream.WithLatestFromBuffered(sampler, (a, b) => a).Subscribe(
				results.Add,
				errors.Add,
				() => completed.Add(Unit.Default));

			stream.OnNext("emitted");

			sampler.OnNext(Unit.Default);
			Assert.AreEqual(1, results.Count, "OnNext");

			sampler.OnNext(Unit.Default);
			Assert.AreEqual(1, results.Count, "OnNext");

			stream.OnNext("emitted2");
			Assert.AreEqual(2, results.Count, "OnNext");

			sampler.OnNext(Unit.Default);
			Assert.AreEqual(2, results.Count, "OnNext");

			stream.OnNext("emitted3");
			Assert.AreEqual(3, results.Count, "OnNext");

			Assert.AreEqual(0, errors.Count, "OnError");

			stream.OnCompleted();

			Assert.AreEqual(1, completed.Count, "OnCompleted");
		}

		[Test]
		public void HotStreamAndColdSampler()
		{
			var stream = new Subject<string>();
			var sampler = Observable.Return("tick");

			var results = new List<string>();
			var errors = new List<Exception>();
			var completed = new List<Unit>();
			stream.WithLatestFromBuffered(sampler, (a, b) => a).Subscribe(
				results.Add,
				errors.Add,
				() => completed.Add(Unit.Default));

			stream.OnNext("emitted");

			Assert.AreEqual(1, results.Count, "OnNext");
			Assert.AreEqual(0, errors.Count, "OnError");

			stream.OnCompleted();

			Assert.AreEqual(1, completed.Count, "OnCompleted");

			Assert.AreEqual("emitted", results[0]);
		}

		[Test]
		public void EmptyStreamAndHotSampler()
		{
			var stream = Observable.Empty<string>();
			var sampler = new Subject<Unit>();

			var results = new List<string>();
			var errors = new List<Exception>();
			var completed = new List<Unit>();
			stream.WithLatestFromBuffered(sampler, (a, b) => a).Subscribe(
				results.Add,
				errors.Add,
				() => completed.Add(Unit.Default));

			sampler.OnNext(Unit.Default);
			sampler.OnNext(Unit.Default);
			sampler.OnNext(Unit.Default);

			Assert.AreEqual(0, results.Count, "OnNext");
			Assert.AreEqual(0, errors.Count, "OnError");

			sampler.OnCompleted();

			Assert.AreEqual(1, completed.Count, "OnCompleted");
		}

		[Test]
		public void ColdStreamAndEmptySampler()
		{
			var stream = Observable.Return("Hello World");
			var sampler = Observable.Empty<Unit>();

			var results = new List<string>();
			var errors = new List<Exception>();
			var completed = new List<Unit>();
			stream.WithLatestFromBuffered(sampler, (a, b) => a).Subscribe(
				results.Add,
				errors.Add,
				() => completed.Add(Unit.Default));

			Assert.AreEqual(0, results.Count, "OnNext");
			Assert.AreEqual(0, errors.Count, "OnError");
			Assert.AreEqual(1, completed.Count, "OnCompleted");
		}

	}
}