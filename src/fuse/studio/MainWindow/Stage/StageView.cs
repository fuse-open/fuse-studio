using System;
using System.Reactive.Linq;
using Outracks.Fuse.Studio;
using Outracks.Fusion;
using LogView = Outracks.Fuse.Studio.LogView;

namespace Outracks.Fuse.Stage
{
	class StageView
	{
		readonly IStage _stage;
		readonly IContext _context;
		readonly IObservable<Mode> _mode;

		public StageView(
			IStage stageController,
			IContext context,
			IObservable<Mode> mode)
		{
			_stage = stageController;
			_context = context;
			_mode = mode;
		}

		public IControl CreateStage(LogView logview, IControl notifications)
		{
			var bottom = UserSettings.Point("BottomPanelHeight").Or(new Points(200));

			var inNormalMode = _mode.Select(x => x == Mode.Normal);

			return inNormalMode.Select(normalMode =>
				normalMode
					? Layout.Dock()
						.Bottom(notifications)
						.Panel(RectangleEdge.Bottom, bottom,
							logview.IsExpanded,
							control: logview.TabContent,
							minSize: 55)
						.Bottom(logview.TabHeader)
						.Left(Primitives.Bar.Clip())
						.Fill(StackedViewports.Clip())
					: Layout.Layer(CompactViewport
						.WithPadding(
							left: MainWindow.ResizeBorderSize,
							top: MainWindow.ResizeBorderSize,
							right: MainWindow.ResizeBorderSize,
							bottom: LogViewHeader.HeaderHeight))
						.WithBackground(Theme.Background)
						.WithNativeOverlay(
							Layout.Dock()
								.Bottom(notifications)
								.Panel(RectangleEdge.Bottom, bottom,
									logview.IsExpanded.Skip(1).StartWith(false),
									control: logview.TabContent,
									minSize: 55)
								.Bottom(logview.TabHeader)
								.Fill()
								.WithPadding(
									left: MainWindow.ResizeBorderSize,
									top: new Points(0),
									right: MainWindow.ResizeBorderSize,
									bottom: MainWindow.ResizeBorderSize)))
				.Switch();
		}

		IControl CompactViewport
		{
			get
			{
				return _stage.FocusedViewport
					.DistinctUntilChanged()
					.SelectPerElement(vp => vp.Control)
					.Or(Control.Empty)
					.Switch();
			}
		}

		IControl StackedViewports
		{
			get
			{
				var deselectArea = Button.Create(
					clicked: Command.Enabled(() => _context.Select(Element.Empty)),
					content: _ => Shapes.Rectangle(fill: Theme.WorkspaceBackground));

				return _stage.Viewports
					.SelectPerElement(EncapsulateView)
					.StackFromLeft()
					.CenterHorizontally()
					.DockTop()
					.WithBackground(deselectArea)
					.MakeScrollable(darkTheme: Theme.IsDark, supportsOpenGL: true, zoomAttributes: new ZoomAttributes(0.5f, 4.0f));
			}
		}

		IControl EncapsulateView(IViewport viewport)
		{
			var deviceSize = viewport.VirtualDevice.Select(s => s.GetOrientedSizeInPoints()).Transpose();
			var deviceNameLabel = Label.Create(
				text: viewport.VirtualDevice.Select(d => d.Screen.Name).AsText(),
				font: Theme.DescriptorFont,
				color: Theme.DescriptorText);

			var focusedProperty = _stage.FocusedViewport
				.Select(maybeViewport => maybeViewport.Select(vp => vp == viewport).Or(false))
				.AsProperty(write: (b, save) => { if (b) viewport.Focus(); });

			var view = Layout.StackFromTop(
				Layout.StackFromLeft(
					FocusBox.Create(focusedProperty).DockBottom(),
					Spacer.Small,
					deviceNameLabel),
				viewport.Control.WithSize(deviceSize).DropShadow(
					color: Theme.Shadow,
					radius: Observable.Return(new Points(5)),
					distance: Observable.Return(new Points(2))))
				.WithPadding(new Thickness<Points>(15));

			return view;
		}
	}
}