using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using System.Reactive.Linq;
using System.Reactive.Subjects;

namespace Outracks.Fusion
{
	public static partial class Layout
	{
		public static IControl StackFromTop(this IObservable<IEnumerable<IControl>> controls, Func<IControl> separator = null)
		{
			if (separator != null) controls = controls.Select(c => c.Join(separator));
			return Stack(Direction2D.TopToBottom, controls);
		}

		public static IControl StackFromTop(IEnumerable<IControl> controls)
		{
			return Stack(Direction2D.TopToBottom, controls);
		}

		public static IControl StackFromTop(params IControl[] controls)
		{
			return Stack(Direction2D.TopToBottom, controls);
		}


		public static IControl StackFromBottom(this IObservable<IEnumerable<IControl>> controls, Func<IControl> separator = null)
		{
			if (separator != null) controls = controls.Select(c => c.Join(separator));
			return Stack(Direction2D.BottomToTop, controls);
		}

		public static IControl StackFromBottom(IEnumerable<IControl> controls)
		{
			return Stack(Direction2D.BottomToTop, controls);
		}

		public static IControl StackFromBottom(params IControl[] controls)
		{
			return Stack(Direction2D.BottomToTop, controls);
		}


		public static IControl StackFromLeft(this IObservable<IEnumerable<IControl>> controls, Func<IControl> separator = null)
		{
			if (separator != null) controls = controls.Select(c => c.Join(separator));
			return Stack(Direction2D.LeftToRight, controls);
		}

		public static IControl StackFromLeft(IEnumerable<IControl> controls)
		{
			return Stack(Direction2D.LeftToRight, controls);
		}

		public static IControl StackFromLeft(params IControl[] controls)
		{
			return Stack(Direction2D.LeftToRight, controls);
		}


		public static IControl StackFromRight(this IObservable<IEnumerable<IControl>> controls, Func<IControl> separator = null)
		{
			if (separator != null) controls = controls.Select(c => c.Join(separator));
			return Stack(Direction2D.RightToLeft, controls);
		}

		public static IControl StackFromRight(IEnumerable<IControl> controls)
		{
			return Stack(Direction2D.RightToLeft, controls);
		}

		public static IControl StackFromRight(params IControl[] controls)
		{
			return Stack(Direction2D.RightToLeft, controls);
		}


		public static IControl Stack(Direction2D direction, IEnumerable<IControl> controls)
		{
			return Stack(direction, controls.ToArray());
		}

		public static IControl Stack(Direction2D direction, params IControl[] controls)
		{
			return Stack(direction, Observable.Return(controls));
		}

		public static IControl Stack(Direction2D direction, IObservable<IEnumerable<IControl>> controls)
		{
			return Stack(direction, controls.Select(d => d.ToArray()));
		}

		public static IControl Stack(Direction2D direction, IObservable<IControl[]> controls)
		{
			controls = controls.Replay(1).RefCount();

			var edge = (RectangleEdge) direction;
			edge = edge.FlipVerticallyOnMac();

			var u = edge.NormalAxis();
			var v = u.Opposite();

			return Layer(self =>
				{
					return controls
						.Scan(
							new { framedControls = (IControl[]) null, cache = ImmutableDictionary<IControl, StackItem>.Empty },
							(state, currentControls) =>
							{
								var frame = ObservableMath.RectangleWithSize(self.NativeFrame.Size);

								var top = frame.GetEdge(edge);

								var newCache = ImmutableDictionary.CreateBuilder<IControl, StackItem>();
								var framedControls = new IControl[currentControls.Length];
								for (int i = 0; i < currentControls.Length; i++)
								{
									var c = currentControls[i];
									StackItem stackItem;
									if (!state.cache.TryGetValue(c, out stackItem))
									{
										stackItem = new StackItem(c, frame, u);
									}
									newCache.Add(c, stackItem);

									var bottom = edge.IsMinimal()
										? top.Add(c.DesiredSize[u])
										: top.Sub(c.DesiredSize[u]);

									bottom = bottom
										.DistinctUntilChanged()
										.Replay(1).RefCount();

									var res = edge.IsMinimal()
										? Interval.FromOffsetLength(top, c.DesiredSize[u])
										: Interval.FromOffsetLength(bottom, c.DesiredSize[u]);

									stackItem.UpdateUAxis(res);

									top = bottom;

									framedControls[i] = stackItem.Control;
								}

								return new { framedControls, cache = newCache.ToImmutable() };
							})
							.Select(x => x.framedControls)
							.Replay(1)
							.RefCount();
				})
				.WithSize(desiredSize:
					Size.Create(
						controls.Select(s => s.Select(ss => ss.DesiredSize[u]))
							.ToObservableEnumerable()
							.Select(l => new Points(l.ConcatOne(0).Sum(p => p.Value))),
						controls.Select(s => s.Select(ss => ss.DesiredSize[v]))
							.ToObservableEnumerable()
							.Select(l => new Points(l.ConcatOne(0).Max(p => p.Value))),
						firstAxis: u));

		}

		class StackItem
		{
			readonly IControl _control;
			readonly Interval<ReplaySubject<IObservable<Points>>> _frameUAxis;

			public IControl Control
			{
				get { return _control; }
			}

			public void UpdateUAxis(Interval<IObservable<Points>> res)
			{
				_frameUAxis.Offset.OnNext(res.Offset);
				_frameUAxis.Length.OnNext(res.Length);
			}

			public StackItem(IControl control, Rectangle<IObservable<Points>> stackFrame, Axis2D u)
			{
				_frameUAxis = new Interval<ReplaySubject<IObservable<Points>>>(
					new ReplaySubject<IObservable<Points>>(1),
					new ReplaySubject<IObservable<Points>>(1));

				var frameUAxisSwitched = _frameUAxis.Select(x => x.Switch().DistinctUntilChanged().Replay(1).RefCount());
				_control = control.WithFrame(stackFrame.WithAxis(u, frameUAxisSwitched));
			}
		}
	}
}