using System;
using System.Reactive.Linq;
using System.Reactive.Subjects;
using Fuse.Preview;

namespace Outracks.Fuse.Stage
{
	using Fusion;
	using Simulator;
	using UnoHost;

	class Gizmos : IPluginFactory
	{
		public static IDisposable Initialize(
			IUnoHostControl unoHost,
			IObservable<bool> selectionEnabled,
			IProject project)
		{
			var context = project.Context;

			var input = Observable.Merge<IBinaryMessage>(
				selectionEnabled.Select(select => new ChangeTool { Tool = select ? Tool.Select : Tool.None, }),
				context.CurrentSelection.SimulatorId.Select(id => new ChangeSelection { IsPreview = false, Id = id }),
				context.PreviewedSelection.SimulatorId.Select(id => new ChangeSelection { IsPreview = true, Id = id }));

			var output = unoHost.LoadPlugin<Gizmos>(input);

			return output
				.TryParse(ChangeSelection.MessageType, ChangeSelection.ReadDataFrom)
				.Select(msg =>
					Observable.FromAsync(async () => 
					{
						if (msg.IsPreview)
							await context.Preview(project.GetElement(msg.Id));
						else
							await context.Select(project.GetElement(msg.Id));
					}))
				.Concat()
				.Subscribe();
		}

		/// <summary>
		/// This method is called by unoHost.LoadPlugin() in the UnoHost process, communication back and forth happens through IBinaryMessages
		/// </summary>
		public Plugin Create(PluginContext context)
		{
			var output = new Subject<ChangeSelection>();

			var actualSelection = 
				context.Input.TryParse(ChangeSelection.MessageType, ChangeSelection.ReadDataFrom)
					.Merge(output)
					.Where(s => !s.IsPreview).Select(s => s.Id)
					.StartWith(ObjectIdentifier.None)
					.DistinctUntilChanged().Replay(1);
			
			var previewSelection = 
				context.Input.TryParse(ChangeSelection.MessageType, ChangeSelection.ReadDataFrom)
					.Merge(output)
					.Where(s => s.IsPreview).Select(s => s.Id)
					.StartWith(ObjectIdentifier.None)
					.DistinctUntilChanged().Replay(1);
		
			var tool = 
				context.Input
					.TryParse(ChangeTool.MessageType, ChangeTool.ReadDataFrom)
					.StartWith(new ChangeTool { Tool = Tool.None })
					.DistinctUntilChanged().Replay(1);

			actualSelection.Connect();
			previewSelection.Connect();

			tool.Connect();

			var selectionEnabled = tool.Select(t => t.Tool == Tool.Select);

			var actualSelectionVisible = selectionEnabled
				.Or(actualSelection
					.Select(_ => Observable.Timer(TimeSpan.FromSeconds(1))
						.ObserveOn(context.Dispatcher)
						.Select(__ => false).StartWith(true))
					.Switch()
					.StartWith(false));

			var previewSelectionVisible = selectionEnabled
				.Or(previewSelection
					.Select(_ => Observable.Timer(TimeSpan.FromSeconds(1))
						.ObserveOn(context.Dispatcher)
						.Select(__ => false).StartWith(true))
					.Switch()
					.StartWith(false));

			return new Plugin
			{
				Output = output,
				Overlay =
					Layout.Layer(
						VisualizeSelection(context, actualSelection).ShowWhen(actualSelectionVisible),
						VisualizePreviewSelection(context, previewSelection).ShowWhen(previewSelectionVisible),
						HitBoxes.Create(actualSelection, output, context).ShowWhen(selectionEnabled))
			};
		}

		static IControl VisualizePreviewSelection(PluginContext context, IObservable<ObjectIdentifier> previewSelection)
		{
			return previewSelection
				.Select(context.GetObjects)
				.Switch()
				.WherePerElement(o => (bool)context.Reflection.IsSubtype(o, "Fuse.Visual"))
				.PoolPerElement(oo =>
					Shapes.Rectangle(
						stroke: Theme.SelectionStroke(
							isSelected: Observable.Return(false),
							isHovering: Observable.Return(true),
							showOutline: Observable.Return(false)))
						.WithFixedPosition(context.GetBounds(oo).Transpose()))
				.Layer();
		}

		static IControl VisualizeSelection(PluginContext context, IObservable<ObjectIdentifier> selection)
		{
			return selection
				.Select(context.GetObjects)
				.Switch()
				.WherePerElement(o => (bool)context.Reflection.IsSubtype(o, "Fuse.Visual"))
				.PoolPerElement(oo =>
					BoxFactory.CreateSpacingBox(
						context.GetBounds(oo),
						context.GetThickness(oo, "Margin"),
						context.GetThickness(oo, "Padding")))
				.Layer();
		}

	}
}