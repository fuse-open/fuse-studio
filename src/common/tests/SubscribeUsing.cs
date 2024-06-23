using System;
using System.Collections.Generic;
using System.Reactive.Subjects;
using NUnit.Framework;

namespace Outracks.Tests
{
	class SubscribeUsing
	{
		[Test]
		public void SubscribeUsingTest01()
		{
			var foo = new Subject<int>();
			var results = new List<int>();
			var sub = foo.SubscribeUsing(
				i =>
				{
					return Disposable.Create(() => results.Add(i));
				});
			foo.OnNext(1);
			foo.OnNext(2);
			foo.OnNext(3);
			sub.Dispose();

			Assert.IsTrue(results.Contains(1), "Expected first element to be disposed.");
			Assert.IsTrue(results.Contains(2), "Expected second element to be disposed.");
			Assert.IsTrue(results.Contains(3), "Expected third element to be disposed.");
		}

		[Test]
		public void SubscribeUsingTest02()
		{
			var foo = new Subject<int>();
			var results = new List<int>();
			foo.SubscribeUsing(
				i =>
				{
					return Disposable.Create(() => results.Add(i));
				});
			foo.OnNext(1);
			foo.OnNext(2);
			foo.OnNext(3);

			Assert.IsTrue(results.Contains(1), "Expected first element to be disposed.");
			Assert.IsTrue(results.Contains(2), "Expected second element to be disposed.");
			Assert.IsFalse(results.Contains(3), "Didn't expect third element to be disposed.");
		}

		[Test]
		public void SubscribeUsingTest03()
		{
			var foo = new Subject<int>();
			var results = new List<int>();
			foo.SubscribeUsing(
				i =>
				{
					return Disposable.Create(() => results.Add(i));
				});
			foo.OnNext(1);
			foo.OnNext(2);
			foo.OnError(new Exception("Haha"));
			foo.OnNext(3);

			Assert.IsTrue(results.Contains(1), "Expected first element to be disposed.");
			Assert.IsFalse(results.Contains(2), "Didn't expect second element to be disposed.");
			Assert.IsFalse(results.Contains(3), "Didn't expect third element to be disposed.");
		}

		[Test]
		public void SubscribeUsingTest04()
		{
			var foo = new Subject<int>();
			var results = new List<int>();
			var sub = foo.SubscribeUsing(
				i =>
				{
					return Disposable.Create(() => results.Add(i));
				});
			foo.OnNext(1);
			foo.OnNext(2);
			foo.OnError(new Exception("Haha"));
			foo.OnNext(3);
			sub.Dispose();

			Assert.IsTrue(results.Contains(1), "Expected first element to be disposed.");
			Assert.IsTrue(results.Contains(2), "Expected second element to be disposed.");
			Assert.IsFalse(results.Contains(3), "Didn't expect third element to be disposed.");
		}

		[Test]
		public void SubscribeUsingTest05()
		{
			var foo = new Subject<int>();
			var results = new List<int>();
			var sub = foo.SubscribeUsing(
				i =>
				{
					return Disposable.Create(() => results.Add(i));
				});
			foo.OnNext(1);
			foo.OnNext(2);
			foo.OnCompleted();
			foo.OnNext(3);
			sub.Dispose();

			Assert.IsTrue(results.Contains(1), "Expected first element to be disposed.");
			Assert.IsTrue(results.Contains(2), "Expected second element to be disposed.");
			Assert.IsFalse(results.Contains(3), "Didn't expect third element to be disposed.");
		}

		[Test]
		public void SubscribeUsingTest06()
		{
			var foo = new Subject<int>();
			var results = new List<int>();
			foo.SubscribeUsing(
				i =>
				{
					return Disposable.Create(() => results.Add(i));
				});
			foo.OnNext(1);
			foo.OnNext(2);
			foo.OnCompleted();
			foo.OnNext(3);

			Assert.IsTrue(results.Contains(1), "Expected first element to be disposed.");
			Assert.IsFalse(results.Contains(2), "Didn't expect second element to be disposed.");
			Assert.IsFalse(results.Contains(3), "Didn't expect third element to be disposed.");
		}
	}
}