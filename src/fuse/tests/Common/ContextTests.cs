using System;
using System.Reactive.Linq;
using System.Threading.Tasks;
using NSubstitute;
using NUnit.Framework;
using Outracks.Fuse.Live;
using Outracks.Simulator;

namespace Outracks.Fuse.Protocol.Tests.Common
{
	public class ContextTests
	{
		[Test]
		public void NewContextHasCorrectScope()
		{
			var element = ElementWithId("1");
			var context = new Context(element, i => ElementWithId(i.ToString()));
			AssertElementsEqualAsync(element, context.CurrentScope);
			AssertNoElement(context.PreviousScope);
		}

		[Test]
		public async Task PushingAndPoppingScopeRetainsHistory()
		{
			var e1 = ElementWithId("1");
			var e2 = ElementWithId("2");
			var e3 = ElementWithId("3");
			var context = new Context(e1, i => ElementWithId(i.ToString()));
			AssertElementsEqualAsync(e1, context.CurrentScope);
			AssertNoElement(context.PreviousScope);

			await context.PushScope(e2, e2);
			AssertElementsEqualAsync(e2, context.CurrentScope);
			AssertElementsEqualAsync(e1, context.PreviousScope);

			await context.PushScope(e3, e3);
			AssertElementsEqualAsync(e3, context.CurrentScope);
			AssertElementsEqualAsync(e2, context.PreviousScope);

			await context.PopScope();
			AssertElementsEqualAsync(e2, context.CurrentScope);
			AssertElementsEqualAsync(e1, context.PreviousScope);

			await context.PopScope();
			AssertElementsEqualAsync(e1, context.CurrentScope);
			AssertNoElement(context.PreviousScope);
		}

		[Test]
		public async Task ResetingScopeClearsHistory()
		{
			var e1 = ElementWithId("1");
			var e2 = ElementWithId("2");
			var e3 = ElementWithId("3");
			var context = new Context(e1, i => ElementWithId(i.ToString()));
			AssertElementsEqualAsync(e1, context.CurrentScope);
			AssertNoElement(context.PreviousScope);

			await context.PushScope(e2, e2);
			AssertElementsEqualAsync(e2, context.CurrentScope);
			AssertElementsEqualAsync(e1, context.PreviousScope);

			await context.PushScope(e2, e2);
			AssertElementsEqualAsync(e2, context.CurrentScope);
			AssertElementsEqualAsync(e2, context.PreviousScope);

			await context.SetScope(e3, e3);
			AssertElementsEqualAsync(e3, context.CurrentScope);
			AssertElementsEqualAsync(e1, context.PreviousScope);
		}

		static IElement ElementWithId(string id)
		{
			var element1 = Substitute.For<IElement>();
			element1.SimulatorId.Returns(Observable.Return(new ObjectIdentifier(id)));
			return element1;
		}

		static void AssertElementsEqualAsync(IElement element1, IElement element2)
		{
			Assert.AreEqual(
				element1.SimulatorId.LastNonBlocking(),
				element2.SimulatorId.LastNonBlocking());
		}

		static void AssertNoElement(IElement element)
		{
			var none = ObjectIdentifier.None;
			element.SimulatorId.Subscribe(id => none = id);
			Assert.AreEqual(ObjectIdentifier.None, none);
		}
	}
}
