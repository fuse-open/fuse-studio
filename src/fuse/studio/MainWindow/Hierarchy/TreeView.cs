using System;
using System.Linq;
using System.Reactive.Linq;
using System.Reactive.Subjects;
using Outracks.Fuse.Studio;
using Outracks.Fusion;

namespace Outracks.Fuse.Hierarchy
{
	public static class TreeView
	{
		static readonly bool ShouldFlip = Axis2DExtensions.ShouldFlip;

		public static IControl Create(ITreeViewModel model)
		{
			var rowHeight = TreeRowView.Height;
			var hoverIndicator = model.PendingDrop.SelectPerElement(
				x => Point.Create(
					Observable.Return(TreeRowView.GetIndentation(x.Depth + (x.DropPosition == DropPosition.Inside ? 1 : 0))),
					Observable.Return<Points>((x.RowOffset + (x.DropPosition != DropPosition.Before ? 1 : 0)) * rowHeight)));

			var rowCount = model.TotalRowCount.Replay(1).RefCount();

			var visibleHeight = new BehaviorSubject<Points>(0);
			var contentHeight = visibleHeight.CombineLatest(rowCount, (vheight, rcount) => (Points)Math.Max(vheight, rcount * rowHeight)).Replay(1).RefCount();

			var flipy = ShouldFlip ? ((Func<IObservable<Points>, IObservable<Points>>) (logicalY => logicalY.CombineLatest(contentHeight, (ly, ch) => ch - ly))) : (y => y);

			var rowOffsetToTop = (Func<IObservable<int>, IObservable<Points>>)
				(rowOffset => flipy(
				rowOffset.Select(offset => new Points(rowHeight.Value * offset))));

			var top = RectangleEdge.Top.FlipVerticallyOnMac();
			//var down = ShouldFlip ? -1.0 : 1.0;
			//var scrollTargetRect =
			//	rowOffsetToTop(model.ScrollTarget)
			//	.Select(rowTop =>
			//		Rectangle.FromPositionSize<Points>(0, 0, 50, 0)
			//		.WithEdge(top, rowTop)
			//		.WithEdge(top.Opposite(), rowTop + rowHeight * down));

			var rowViews = model.VisibleRows
				.CachePerElement(rowModel => CreateRow(rowModel, rowOffsetToTop))
				.Replay(1)
				.RefCount();

			var background =
				rowViews.SelectPerElement(x => x.Background).Layer();
			var foreground =
				rowViews.SelectPerElement(x => x.Foreground).Layer();
			var overlay =
				rowViews.SelectPerElement(x => x.Overlay).Layer();

			var width =
				rowViews.Select(
					rows => rows.Select(row => row.Foreground.DesiredSize.Width)
								.CombineLatest()
								.Select(widths => widths.ConcatOne(0).Max()))
				.Switch()
				.DistinctUntilChanged();

			var treeView =
				Layout.Layer(background, foreground, overlay)
					.WithBackground(Theme.PanelBackground)
					.WithOverlay(InsertionRod.Create(hoverIndicator))
					.WithHeight(contentHeight)
					.WithWidth(width)
					.MakeScrollable(
						darkTheme: Theme.IsDark,
						// scrollToRectangle: scrollTargetRect,
						onBoundsChanged: bounds =>
						{
							var rowTop = bounds.Visible.GetEdge(top);
							rowTop = ShouldFlip ? bounds.Content.Height - rowTop : rowTop;
							var rowOffset = (int) Math.Floor(rowTop / rowHeight);
							model.VisibleRowOffset = rowOffset;
							var maxVisibleRowCount = (int) Math.Ceiling(bounds.Visible.VerticalInterval.Length / rowHeight.Value) + 1;
							model.VisibleRowCount = maxVisibleRowCount;
							visibleHeight.OnNext(bounds.Visible.Height);
						});
					// TODO: CAN'T GET LOCAL KEYS TO WORK PROPERLY
					//.OnKeyPressed(ModifierKeys.None, Key.Up, model.NavigateUpCommand, isGlobal: true)
					//.OnKeyPressed(ModifierKeys.None, Key.Down, model.NavigateDownCommand, isGlobal: true);

			return CreatePopScopeButton(model).DockTop(fill: treeView);
		}

		static IControl CreatePopScopeButton(ITreeViewModel treeViewModel)
		{
			var popScopeButton =
				treeViewModel.PopScopeCommand.IsEnabled.Select(
					enabled => enabled
						? Button.Create(
							treeViewModel.PopScopeCommand,
							state => Layout.StackFromTop(
								Spacer.Medium,
								Layout.StackFromLeft(
										Arrow.WithShaft(RectangleEdge.Left, brush: Theme.Purple)
											.WithSize(new Size<Points>(15, 7.5))
											.CenterVertically(),
										Spacer.Small,
										Label.Create(
												text: treeViewModel.PopScopeLabel.AsText(),
												font: Theme.DefaultFont,
												color: Theme.DefaultText)
											.CenterVertically())
									.CenterHorizontally(),
								Spacer.Medium))
						: Spacer.Medim).Switch();
			return popScopeButton;
		}

		static TreeRowView CreateRow(ITreeRowViewModel model, Func<IObservable<int>, IObservable<Points>> rowOffsetToTop)
		{
			return new TreeRowView(model, rowOffsetToTop);
		}
	}
}
