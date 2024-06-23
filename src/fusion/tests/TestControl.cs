using System;
using System.Reactive.Linq;
using System.Reactive.Subjects;
using System.Reactive.Threading.Tasks;
using NUnit.Framework;
using Outracks.Diagnostics;

namespace Outracks.Fusion.Tests
{
	public class TestControl : IControl
	{
		readonly string _name;

		public TestControl(string name, Points width, Points height)
		{
			_name = name;
			var mountLocation = new ReplaySubject<IMountLocation>(1);

			Mount = mountLocation.OnNext;
			IsRooted = mountLocation.Switch(p => p.IsRooted);
			NativeHandle = null;
			DesiredSize = Size.Create(
				Observable.Return(new Points(width)),
				Observable.Return(new Points(height)));

			mountLocation.Switch(p => p.AvailableSize)
				.Transpose().Subscribe(r => _available = r);
			mountLocation.Switch(p => p.NativeFrame)
				.Transpose().Subscribe(r => _rectangle = r);
		}

		public Rectangle<IObservable<Points>> NativeFrame { get; set; }


		public Action<IMountLocation> Mount { get; private set; }
		public IObservable<bool> IsRooted { get; private set; }


		public Size<IObservable<Points>> DesiredSize { get; set; }
		public object NativeHandle { get; private set; }

		Optional<Rectangle<Points>> _rectangle = Optional.None();
		Optional<Size<Points>> _available = Optional.None();

		public void AssertAvailableSize(Points width, Points height)
		{
			Assert.IsTrue(_available.HasValue, _name.Capitalize() + " have not receieved an available size");
			Assert.AreEqual(width, _available.Value.Width, _name.Capitalize() + " got wrong available width");
			Assert.AreEqual(height, _available.Value.Height, _name.Capitalize() + " got wrong available height");

		}

		public void AssertFrame(Points left, Points top, Points right, Points bottom)
		{
			var actualTop = _rectangle.Value.Top();
			var actualBottom = _rectangle.Value.Bottom();

			if (Platform.IsMac)
			{
				actualTop = 4 - _rectangle.Value.Bottom();
				actualBottom = 4 - _rectangle.Value.Top();
			}

			Assert.IsTrue(_rectangle.HasValue, _name.Capitalize() + " has not receieved a frame");
			Assert.AreEqual(left, _rectangle.Value.Left(), _name.Capitalize() + " has wrong Left value");
			Assert.AreEqual(top, actualTop, _name.Capitalize() + " has wrong Top, value");
			Assert.AreEqual(right, _rectangle.Value.Right(), _name.Capitalize() + " has wrong Right value");
			Assert.AreEqual(bottom, actualBottom, _name.Capitalize() + " has wrong Bottom value");
		}



	}

	static class NodeExtensions
	{
		/*public static void AssertVisualSelf(this IControl control, IControl visualSelf)
		{
			Assert.AreEqual(visualSelf, control.VisualSelf.FirstAsync().ToTask().Result);
		}

		public static void AssertVisualParent(this IControl control, Optional<IControl> visualParent)
		{
			Assert.AreEqual(visualParent, control.VisualParent.FirstAsync().ToTask().Result);
		}*/

		public static void MountRoot(this IControl control)
		{
			var size = Size.Create(Observable.Return(new Points(4)), Observable.Return(new Points(4)));
			control.Mount(
				new MountLocation.Mutable
				{
					IsRooted = Observable.Return(true),
					AvailableSize = size,
					NativeFrame= ObservableMath.RectangleWithSize(size),
				});

			var foo = control.NativeHandle;
		}

		public static void AssertHasNativeParent(this TestControl control)
		{
			//Assert.AreEqual(NativeParent, control.NativeParent().FirstAsync().ToTask().GetResultAndUnpackExceptions().Value);
		}

		public static void AssertDesiredSize(this IControl control, string name, Points width, Points height)
		{
			Assert.AreEqual(width, control.DesiredSize.Width.FirstAsync().ToTask().GetResultAndUnpackExceptions(), name.Capitalize() + " has wrong desired width");
			Assert.AreEqual(height, control.DesiredSize.Height.FirstAsync().ToTask().GetResultAndUnpackExceptions(), name.Capitalize() + " has wrong desired height");
		}

	}
}