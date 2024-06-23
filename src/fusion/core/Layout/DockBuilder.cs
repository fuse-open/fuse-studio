using System;
using System.Collections.Generic;
using System.Reactive.Linq;

namespace Outracks.Fusion
{
	public static partial class Layout
	{
		public static DockBuilder Dock()
		{
			return DockBuilder.Empty;
		}
	}

	public class DockBuilder
	{
		public static DockBuilder Empty
		{
			get { return new DockBuilder(); }
		}

		public DockBuilder Left(IControl control)
		{
			return Dock(control, RectangleEdge.Left);
		}

		public DockBuilder Right(IControl control)
		{
			return Dock(control, RectangleEdge.Right);
		}

		public DockBuilder Top(IControl control)
		{
			return Dock(control, RectangleEdge.Top);
		}

		public DockBuilder Bottom(IControl control)
		{
			return Dock(control, RectangleEdge.Bottom);
		}
		public DockBuilder Dock(IControl control, RectangleEdge edge)
		{
			return new DockBuilder(this, control, edge);
		}

		readonly Optional<DockBuilder> _parent;
		readonly Optional<IControl> _control;
		readonly RectangleEdge _edge;

		DockBuilder(DockBuilder parent = null, IControl control = null, RectangleEdge edge = default(RectangleEdge))
		{
			_parent = parent.ToOptional();
			_control = control.ToOptional();
			_edge = edge.FlipVerticallyOnMac();
		}
		/*
	public IControl Fill(IControl fill = null)
	{
		var self = _control.Select(c => c.Dock(_edge, fill)).Or(fill);
		var root = _parent.Select(p => p.Fill(self)).Or(self);
		return root ?? Control.Empty;
	}
	*/
	public IControl Fill(IControl fill = null)
	{
		var stack = new Stack<DockBuilder>();
		var db = Optional.Some(this);
		while (db.HasValue)
		{
			stack.Push(db.Value);
			db = db.Value._parent;
		}

		return Layout.Layer(
			(isRooted, availableSize, parentFrame) =>
			{
				var frame = ObservableMath.RectangleWithSize(parentFrame.Size);

				var controls = new List<IControl>();
				while (stack.Count != 0)
				{
					var dock = stack.Pop();
					if (!dock._control.HasValue)
						continue;

					var axis = dock._edge.NormalAxis();
					var isMinimal = dock._edge.IsMinimal();
					var desired = dock._control.Value.DesiredSize[axis];

					var total = frame[axis]
						.DistinctUntilChanged()
						.Replay(1).RefCount()
						;

					var left = total.Offset
						.DistinctUntilChanged()
						.Replay(1).RefCount()
						;

					var right = total.Offset.Add(total.Length)
						.DistinctUntilChanged()
						.Replay(1).RefCount()
						;

					var dockedLength = desired.Min(total.Length)
						.DistinctUntilChanged()
						.Replay(1).RefCount()
						;

					var dockedOffset = (isMinimal ? left : right.Sub(dockedLength))
						.DistinctUntilChanged()
						.Replay(1).RefCount()
						;

					var filledLength = total.Length.Sub(dockedLength)
						.DistinctUntilChanged()
						.Replay(1).RefCount()
						;

					var filledOffset = (isMinimal ? left.Add(dockedLength) : left)
						.DistinctUntilChanged()
						.Replay(1).RefCount()
						;

					controls.Add(dock._control.Value
						.WithFrame(
							frame: frame.WithAxis(axis, old => Interval.FromOffsetLength(dockedOffset, dockedLength))
								.DistinctUntilChanged()
								.Replay(1).RefCount(),
							availableSize: availableSize));

					frame = frame
						.WithAxis(axis, old => Interval.FromOffsetLength(filledOffset, filledLength))
						.DistinctUntilChanged()
						.Replay(1).RefCount()
						;

					availableSize = availableSize
						.WithAxis(axis, old => old.Sub(dockedLength))
						.Max(ObservableMath.ZeroSize)
						.DistinctUntilChanged()
						.Replay(1).RefCount()
						;
				}

				if (fill != null)
					controls.Add(fill.WithFrame(frame, availableSize));

				return controls;
			}).WithSize(DesiredSize(
				fill == null
					? ObservableMath.ZeroSize
					: fill.DesiredSize));
	}
		Size<IObservable<Points>> DesiredSize(Size<IObservable<Points>> fillSize)
		{
			var u = _edge.NormalAxis();
			var v = u.Opposite();

			if (!_control.HasValue)
				return fillSize;

			fillSize = Size.Create(
				_control.Value.DesiredSize[u].Add(fillSize[u]),
				_control.Value.DesiredSize[v].Max(fillSize[v]),
				firstAxis: u);

			if (!_parent.HasValue)
				return fillSize;

			return _parent.Value.DesiredSize(fillSize);
		}
	}
}