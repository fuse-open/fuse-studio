using System;
using System.Collections.Generic;
using System.Linq;
using System.Reactive.Subjects;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Threading;
using NUnit.Framework;
using NUnit.Framework.Internal;
using System.Reactive.Concurrency;

namespace Outracks.Fusion.IntegrationTests
{
	[Apartment(ApartmentState.STA)]
	[TestFixture]
	[Ignore("Tests are failing on TC, and has to be investigated.")]
	public class ControlLeakTests
	{
		internal static IApplication _application;

		[SetUp]
		public void SetUp()
		{
			Console.WriteLine("Thread " + Thread.CurrentThread.ManagedThreadId);
			if (_application == null)
			{
				_application = Application.Initialize(new List<string>());
			}
		}

		static void RunInMainThread(Action action)
		{
			ManualResetEvent waitHandle = new ManualResetEvent(false);
			Exception innerException = null;
			var outerContext = TestExecutionContext.CurrentContext;

			Application.MainThread.Schedule(() =>
			{
				if (TestExecutionContext.CurrentContext == null)
				{
					var innerContext = new TestExecutionContext(outerContext);
					innerContext.EstablishExecutionEnvironment();
				}
				try
				{
					action();
				}
				catch (Exception e)
				{
					innerException = e;
				}
				waitHandle.Set();
			});
			waitHandle.WaitOne();
			if (innerException != null)
			{
				throw new InvalidOperationException("Got exception while running in main thread", innerException);
			}
		}

		[Test]
		public void Rectangle_is_unsubscribed_after()
		{
			RunInMainThread(() =>
			{
				var stroke = new BehaviorSubject<Stroke>(Stroke.Empty);
				var brush = new BehaviorSubject<Brush>(Brush.Transparent);
				var cornerRadius = new BehaviorSubject<CornerRadius>(CornerRadius.None);

				var rectangle = Shapes.Rectangle(stroke.Switch(), brush.Switch(), cornerRadius);
				Assert.That(stroke.HasObservers, Is.False);
				Assert.That(brush.HasObservers, Is.False);
				Assert.That(cornerRadius.HasObservers, Is.False);
				rectangle.MountRoot();
				Assert.That(stroke.HasObservers, Is.True);
				Assert.That(brush.HasObservers, Is.True);
				Assert.That(cornerRadius.HasObservers, Is.True);
				rectangle.Mount(MountLocation.Unmounted);
				Assert.That(brush.HasObservers, Is.False);
				Assert.That(stroke.HasObservers, Is.False);
				Assert.That(cornerRadius.HasObservers, Is.False);
			});
		}

		[Test]
		public void Label_is_unsubscribed_after()
		{
			RunInMainThread(() =>
			{
				var text = new BehaviorSubject<string>("boom");
				var label = Label.Create(text.AsText());
				Assert.That(text.HasObservers, Is.False);
				label.MountRoot();
				Assert.That(text.HasObservers, Is.True);
				label.Mount(MountLocation.Unmounted);
				Assert.That(text.HasObservers, Is.False);
			});
		}

		[Test]
		public void Button_is_unsubscribed_after()
		{
			RunInMainThread(() =>
			{
				var command = new BehaviorSubject<Command>(Command.Enabled(action: () => { }));
				var button = Button.Create(command.Switch());
				Assert.That(command.HasObservers, Is.False);
				button.MountRoot();
				Assert.That(command.HasObservers, Is.True);
				button.Mount(MountLocation.Unmounted);
				Assert.That(command.HasObservers, Is.False);
			});
		}

		[Test]
		public void LayoutTracker_leak_test()
		{
			RunInMainThread(() =>
			{

				AssertIsGarbageCollected(
					() =>
					{
						BehaviorSubject<IEnumerable<IControl>> innerControls =
							new BehaviorSubject<IEnumerable<IControl>>(Enumerable.Empty<IControl>());
						var container = LayoutTracker.Create(
							x => innerControls.CachePerElement(y => x.TrackVisualBounds(frame => { }, y)).StackFromLeft());
						container.MountRoot();
						return new { innerControls, container };
					},
					env =>
					{
						var innerControls = env.innerControls;
						var rectangleHeight = new BehaviorSubject<Points>(33);
						var rectangle = Shapes.Rectangle(fill: Brush.Transparent).WithWidth(rectangleHeight);
						Assert.That(rectangleHeight.HasObservers, Is.False);
						innerControls.OnNext(new[] { rectangle });
						Assert.That(rectangleHeight.HasObservers, Is.True);
						innerControls.OnNext(Enumerable.Empty<IControl>());
						Assert.That(rectangleHeight.HasObservers, Is.False);
						return rectangle;
					});
			});
		}


		[Test]
		public void Stack_leak_test()
		{
			RunInMainThread(() =>
			{

				AssertIsGarbageCollected(() =>
					{
						BehaviorSubject<IEnumerable<IControl>> innerControls =
							new BehaviorSubject<IEnumerable<IControl>>(Enumerable.Empty<IControl>());
						var container = innerControls.StackFromLeft();
						container.MountRoot();
						return new { innerControls, container };
					},
					x =>
					{
						var innerControls = x.innerControls;
						var rectangleHeight = new BehaviorSubject<Points>(33);
						var rectangle = Shapes.Rectangle(fill: Brush.Transparent);
						rectangle = rectangle.WithWidth(rectangleHeight);
						Assert.That(rectangleHeight.HasObservers, Is.False);
						innerControls.OnNext(new[] { rectangle });
						Assert.That(rectangleHeight.HasObservers, Is.True);
						innerControls.OnNext(Enumerable.Empty<IControl>());
						Assert.That(rectangleHeight.HasObservers, Is.False);
						return rectangle;
					});
			});
		}

		[MethodImpl(MethodImplOptions.NoInlining)]
		static WeakReference InvokeAndReturnWeakRef<TEnv>(TEnv env, Func<TEnv, object> func)
		{
			return new WeakReference(func(env));
		}

		/// <summary>
		/// Check that the object returned from the <paramref name="garbageFunc"/> delegate do not outlive
		/// environment set up by <paramref name="environmentFactory"/>
		/// </summary>
		/// <typeparam name="TEnv">Type of environment</typeparam>
		/// <param name="environmentFactory">Environment, which is kept alive while during forced GC</param>
		/// <param name="garbageFunc">Object that we will assert whether it gets garbage collected or not</param>
		[MethodImpl(MethodImplOptions.NoInlining)]
		static void AssertIsGarbageCollected<TEnv>(Func<TEnv> environmentFactory, Func<TEnv, object> garbageFunc)
		{
			Assert.That(
				garbageFunc.Target.GetType().GetFields(BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance).Length,
				Is.EqualTo(0),
				"Delegate should not capture anything");
				
			var env = environmentFactory();
			AssertIsGarbageCollected(env, garbageFunc);
			GC.KeepAlive(env);
		}

		[MethodImpl(MethodImplOptions.NoInlining)]
		static void AssertIsGarbageCollected<TEnv>(TEnv env, Func<TEnv, object> func)
		{
			// We don't have control over (temporary) local variables in function
			// so we have to make and return the weak reference from another method.
			var weakref = InvokeAndReturnWeakRef(env, func);
			GC.Collect(GC.MaxGeneration, GCCollectionMode.Forced, true);
			Thread.Sleep(100);
			GC.Collect(GC.MaxGeneration, GCCollectionMode.Forced, true);
			Assert.That(weakref.IsAlive, Is.False, "Object that was expected collected survied. It should have DIED!");
		}
	}
}