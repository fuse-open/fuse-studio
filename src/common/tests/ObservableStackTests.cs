using System;
using System.Reactive.Linq;
using System.Threading.Tasks;
using NUnit.Framework;

namespace Outracks.Tests
{
	class ObservableStackTests
	{
		[Test]
		public async Task EmptyStack()
		{
			var stack = new ObservableStack<int>();

			Assert.Throws<InvalidOperationException>(() => { var a = stack.Value; });
			Assert.AreEqual(Optional.None<int>(), await stack.Peek.FirstAsync());
			Assert.AreEqual(Optional.None<int>(), await stack.PeekUnder.FirstAsync());
		}

		[Test]
		public async Task PushPopAndPushAgain()
		{
			var stack = new ObservableStack<int>();

			// Push one element
			stack.Push(1);
			Assert.AreEqual(1, stack.Value);
			Assert.AreEqual(Optional.Some(1), await stack.Peek.FirstAsync());
			Assert.AreEqual(Optional.None<int>(), await stack.PeekUnder.FirstAsync());

			// Pop it back to empty
			var pop1 = stack.Pop();
			Assert.AreEqual(1, pop1);
			Assert.Throws<InvalidOperationException>(() => { var a = stack.Value; });
			Assert.AreEqual(Optional.None<int>(), await stack.Peek.FirstAsync());
			Assert.AreEqual(Optional.None<int>(), await stack.PeekUnder.FirstAsync());

			// Push two elements
			stack.Push(1);
			stack.Push(2);
			Assert.AreEqual(2, stack.Value);
			Assert.AreEqual(Optional.Some(2), await stack.Peek.FirstAsync());
			Assert.AreEqual(Optional.Some(1), await stack.PeekUnder.FirstAsync());
		}

		[Test]
		public async Task Replace()
		{
			var stack = new ObservableStack<int>();

			// Initialize stack to be replaced
			stack.Push(13);
			stack.Push(37);

			// Replace all elements
			stack.Replace(1, 2, 3);
			Assert.AreEqual(Optional.Some(3), await stack.Peek.FirstAsync());
			Assert.AreEqual(Optional.Some(2), await stack.PeekUnder.FirstAsync());

			// Pop them off again
			stack.Pop();
			Assert.AreEqual(Optional.Some(2), await stack.Peek.FirstAsync());
			Assert.AreEqual(Optional.Some(1), await stack.PeekUnder.FirstAsync());
			stack.Pop();
			Assert.AreEqual(Optional.Some(1), await stack.Peek.FirstAsync());
			Assert.AreEqual(Optional.None<int>(), await stack.PeekUnder.FirstAsync());
			stack.Pop();
			Assert.AreEqual(Optional.None<int>(), await stack.Peek.FirstAsync());
			Assert.AreEqual(Optional.None<int>(), await stack.PeekUnder.FirstAsync());

			// Push after it was cleared
			stack.Push(1);
			Assert.AreEqual(1, stack.Value);
			Assert.AreEqual(Optional.Some(1), await stack.Peek.FirstAsync());
			Assert.AreEqual(Optional.None<int>(), await stack.PeekUnder.FirstAsync());
		}
	}
}
