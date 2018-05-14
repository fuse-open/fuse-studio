using System;
using System.Collections.Generic;
using System.Linq;
using System.Reactive.Linq;
using System.Reactive.Subjects;
using System.Threading;
using System.Threading.Tasks;
using NUnit.Framework;

namespace Outracks.Fusion.Tests
{
	public class ControlMemoryOverheadTests
	{
		private double MemBenchmark<T>(Func<T> func, int iterations = 128)
		{
			return MemBenchmark(() => (object) null, _ => func(), iterations);
		}

		private double MemBenchmark<TInit, T>(Func<TInit> initFunc, Func<TInit, T> func, int iterations = 1024)
		{
			var instances = new List<T>(iterations + 1) { func(initFunc()) };
			var initials = Enumerable.Range(0, iterations).Select(_ => initFunc()).ToList();
			var preMemory = GC.GetTotalMemory(true);
			for (int i = 0; i < iterations; i++)
				instances.Add(func(initials[i]));
			var memIncrease = (GC.GetTotalMemory(true) - preMemory) / (double) iterations;
			Console.WriteLine("Size of each instance is around " + memIncrease);
			Assert.That(instances, Is.Not.Null); // Just to pin instances until measure is complete
			return memIncrease;
		}

		[Test]
		public void BindNativeProperty_memory_usage()
		{
			Assert.That(MemBenchmark(() =>
				{
					var isRooted = Observable.Return(true);
					var source = new BehaviorSubject<int>(1234);
					var dispatcher = new PollingDispatcher(Thread.CurrentThread);
					var mountLocation = CreateMountLocation();
					return new { isRooted, source, dispatcher, mountLocation };
				},
				x =>
				{
					PropertyBindingExtensions.BindNativeProperty(x.isRooted, x.dispatcher, String.Empty, x.source, v => { /*Console.WriteLine("Got update " + v);*/ });
					return x;
				}), Is.LessThan(1500));
		}

		[Test]
		public void ControlCreate_and_mount_memory_usage()
		{
			MemBenchmark(
				() =>
				{
					var control = Control.Create(ml => null);
					control.Mount(
						CreateMountLocation());
					return control;
				});
		}

		private static MountLocation.Mutable CreateMountLocation()
		{
			return new MountLocation.Mutable
			{
				AvailableSize = Size.Create(Observable.Return(new Points(123)), Observable.Return(new Points(30))),
				IsRooted = Observable.Return(true),
				NativeFrame = CreateNativeFrame()
			};
		}

		[Test]
		public void ControlCreate_no_mount_memory_usage()
		{
			MemBenchmark(() => Control.Create(ml => null));
		}

		[Test]
		public void StackPanel_with_different_number_of_items_memory_usage()
		{
			// This reflects _just_ the overhead of keeping the controls in the stack, as the controls are created before measuring begins
			var emptyStackSize = MemBenchmark(() => new IControl[] { }, Layout.StackFromLeft);
			Console.WriteLine("Empty stack size: " + emptyStackSize);
			var stackWithOneElementSize = MemBenchmark(
				() => Enumerable.Range(0, 1).Select(_ => Control.Create(ml => null)).ToArray(),
				Layout.StackFromLeft);
			var firstElementStackPanelOverhead = stackWithOneElementSize - emptyStackSize;
			Console.WriteLine("Stack with 1 element: {0} (+{1})", stackWithOneElementSize, firstElementStackPanelOverhead);
			var stackWithTwoElementsSize = MemBenchmark(
				() => Enumerable.Range(0, 2).Select(_ => Control.Create(ml => null)).ToArray(),
				Layout.StackFromLeft);
			Console.WriteLine(
				"Stack with 2 element: {0} (+{1})",
				stackWithTwoElementsSize,
				stackWithTwoElementsSize - stackWithOneElementSize);
			Assert.That(stackWithTwoElementsSize, Is.LessThan(7000));
			var stackWithThreeElementsSize = MemBenchmark(
				() => Enumerable.Range(0, 3).Select(_ => Control.Create(ml => null)).ToArray(),
				Layout.StackFromLeft);
			Console.WriteLine(
				"Stack with 3 element: {0} (+{1})",
				stackWithThreeElementsSize,
				stackWithThreeElementsSize - stackWithTwoElementsSize);
		}

		[Test]
		public void WithPadding_all_sides_memory_usage()
		{
			MemBenchmark(
				() => Control.Create(ml => null),
				c => c.WithPadding(new Thickness<Points>(40)));
		}

		[Test]
		public void Control_switch_memory_usage()
		{
			MemBenchmark(() => Observable.Return(Control.Create(ml => null)), c => c.Switch());
		}

		[Test]
		public void Control_switch_mount_memory_usage()
		{
			MemBenchmark(
				() => Observable.Return(Control.Create(ml => null)).Switch(),
				c =>
				{
					c.Mount(CreateMountLocation());
					return c;
				});
		}

		private static Rectangle<IObservable<Points>> CreateNativeFrame()
		{
			return Rectangle.FromPositionSize(
				Observable.Return(new Points(4)),
				Observable.Return(new Points(5)),
				Observable.Return(new Points(100)),
				Observable.Return(new Points(100)));
		}
	}
}
