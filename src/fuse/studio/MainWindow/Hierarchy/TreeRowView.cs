using System;
using System.Reactive.Linq;
using Outracks.Fuse.Studio;
using Outracks.Fusion;

namespace Outracks.Fuse.Hierarchy
{
	class TreeRowView
	{
		readonly ITreeRowViewModel _model;

		readonly IObservable<Points> _indentation;

		public IControl Background { get; private set; }
		public IControl Foreground { get; private set; }
		public IControl Overlay { get; private set; }

		public TreeRowView(ITreeRowViewModel model, Func<IObservable<int>, IObservable<Points>> rowOffsetToTop)
		{
			_model = model;
			_indentation = model.Depth.Select(GetIndentation).Replay(1).RefCount();

			var top = RectangleEdge.Top.FlipVerticallyOnMac();
			var down = Axis2DExtensions.ShouldFlip ? -1.0 : 1.0;

			var frameVerticalInterval = rowOffsetToTop(model.RowOffset)
				.CombineLatest(
					_model.ExpandedDescendantCount,
					(rowTop, expandedDescendantCount) =>
						Rectangle.Zero<Points>()
							.WithEdge(top, rowTop)
							.WithEdge(top.Opposite(), rowTop.Add((expandedDescendantCount + 1) * Height * down))
							.VerticalInterval)
				.Transpose()
				.Replay(1)
				.RefCount();

			Background =
				VerticalLine(Theme.OutlineVerticalLineBackground)
					.WithFrame(x => x.WithAxis(Axis2D.Vertical, frameVerticalInterval));

			Foreground =
				Header()
					.WithBackground(HitRect())
					.WithBackground(SelectionIndicator())
					.DockTop()
					.WithFrame(x => x.WithAxis(Axis2D.Vertical, frameVerticalInterval));

			var highlightBrush =
				_model.IsFaded.Select(isFaded => isFaded ? Theme.PanelBackground.WithAlpha(0.5f) : Color.Transparent).Switch();

			Overlay =
				VerticalLine(Color.FromBytes(255, 255, 255, 30))
					.WithOverlay(Shapes.Rectangle(fill: highlightBrush).WithHeight(Height).DockTop())
					.WithFrame(x => x.WithAxis(Axis2D.Vertical, frameVerticalInterval));
		}

		IControl VerticalLine(Brush color)
		{
			return Separator.HorizontalLine(color, new Points(1))
				.CenterHorizontally()
				.WithWidth(IconWidth)
				.WithPadding(left: Indentation, top: Observable.Return(Height - IconPadding), bottom: Observable.Return(IconPadding))
				.DockLeft();
		}

		IControl SelectionIndicator()
		{
			return Shapes.Rectangle(
					fill: _model.IsSelected.Select(selected => selected ? Theme.Active : Theme.Active.WithAlpha(0.11f)).Switch())
				.ShowWhen(_model.IsSelected.Or(_model.IsAncestorSelected));
		}

		IControl Header()
		{
			return Layout
				.StackFromLeft(
					ElementIcon().CenterVertically(),
					Spacer.Small,
					Label.Create(HeaderText.AsText(), HeaderTextFont, color: HeaderTextColor)
						.CenterVertically(),
					Spacer.Small,
					ExpandCollapseButton())
				.WithPadding(left: Indentation)
				.WithHeight(Height);
		}

		IControl ElementIcon()
		{
			return _model.ElementName.Where(x => x != "").StartWith("").DistinctUntilChanged()
				.Select(x => Icons.MediumIcon(x, IconColor, IconColor)).Switch();
		}

		IControl ExpandCollapseButton()
		{
			var expandedArrow = Arrow.WithoutShaft(RectangleEdge.Bottom, SymbolSize.Medium, ExpansionArrowColor)
				.Center()
				.WithSize(Size.Create<Points>(11, 11))
				.VisibleWhen(_model.IsExpanded);

			var collapsedArrow = Arrow.WithoutShaft(RectangleEdge.Right, SymbolSize.Medium, ExpansionArrowColor)
				.Center()
				.WithSize(Size.Create<Points>(11, 11))
				.VisibleWhen(_model.IsExpanded.IsFalse());

			var expandCollapseButton =
				new[] { expandedArrow, collapsedArrow }.Layer()
					.WithSize(Size.Create<Points>(11, 11))
					.OnMouse(pressed: _model.ExpandToggleCommand)
					.CenterVertically()
					.VisibleWhen(_model.ExpandToggleCommand.IsEnabled);
			//.ShowWhen(_model.ShowExpandArrow);
			return expandCollapseButton;
		}

		IObservable<string> HeaderText
		{
			get { return _model.HeaderText; }
		}

		Font HeaderTextFont
		{
			get { return _model.Depth.Select(depth => depth == 0 ? Theme.DefaultFontBold : Theme.DefaultFont).Switch(); }
		}

		Brush HeaderTextColor
		{
			get { return _model.IsSelected.Select(s => s ? Color.White : Theme.DefaultText).Switch(); }
		}

		Brush ExpansionArrowColor
		{
			get { return _model.IsSelected.Select(s => s ? Color.White : Theme.Active).Switch(); }
		}

		Brush IconColor
		{
			get { return _model.IsSelected.Select(s => s ? Color.White : Theme.OutlineIcon).Switch(); }
		}

		// Input event handling (click/drag/hover)
		IControl HitRect()
		{
			return Layout.SubdivideVertically(
				HitRectPart(DropPosition.Before),
				HitRectPart(DropPosition.Inside),
				HitRectPart(DropPosition.After));
		}


		IControl HitRectPart(DropPosition position)
		{
			return Shapes.Rectangle()
					.OnDragOver(
						canDrop: data => _model.CanDrop(position, data),
						enter: data => _model.DragEnter(position),
						leave: data => _model.DragExit(),
						drop: data => _model.Drop(position, data))
					.OnMouse(
						entered: _model.EnterHoverCommand,
						exited: _model.ExitHoverCommand,
						pressed: _model.SelectCommand,
						dragged: _model.DraggedObject,
						doubleClicked: _model.ScopeIntoClassCommand)
					.SetContextMenu(_model.ContextMenu)
				;
		}

		// Metrics

		public static Points GetIndentation(int depth)
		{
			return new Points(10 + depth * 25);
		}

		IObservable<Points> Indentation
		{
			get { return _indentation; }
		}

		public static readonly Points Height = new Points(23);
		static readonly Points IconPadding = new Points(5);
		static readonly Points IconWidth = new Points(21);
	}

	static class VisibleWhenExtension
	{
		public static IControl VisibleWhen(this IControl control, IObservable<bool> condition)
		{
			return control.WithFrame(f => f.WithAxis(Axis2D.Horizontal, interval => Interval.FromOffsetLength(interval.Offset.CombineLatest(condition, (x, isVisible) => isVisible ? x : -10000), interval.Length)));
		}
	}
}
