using System;
using System.Collections.Generic;
using System.Linq;
using System.Reactive.Linq;

namespace Outracks.Fuse.Stage
{
	using Fusion;
	using Simulator;
	using Simulator.Runtime;
	using UnoHost;
	using Uno;

	static class HitBoxes
	{
		public static IControl Create(IObservable<ObjectIdentifier> selection, IObserver<ChangeSelection> nextSelection, PluginContext ctx)
		{
			var ctrl = Shapes.Rectangle().MakeHittable();

			ChangeSelectionOnClick(ctrl, selection, ctx).Subscribe(nextSelection.OnNext);

			return ctrl.Control;
		}

		static IObservable<ChangeSelection> ChangeSelectionOnClick(Hittable ctrl, IObservable<ObjectIdentifier> selection, PluginContext ctx)
		{
			return Observable.Merge(
				ctrl.Moved
					.WithLatestFromBuffered(selection, (pos, currentSelection) =>
						new ChangeSelection
						{
							IsPreview = true,
							Id = HitTest(pos, currentSelection, ctx),
						}),

				ctrl.Pressed
					.WithLatestFromBuffered(selection, (pos, currentSelection) =>
					{
						var newSelection = HitTest(pos, currentSelection, ctx);
						var newPreviewSelection = HitTest(pos, newSelection, ctx);
						return new { newSelection, newPreviewSelection };
					})
					.SelectMany(t => new[] 
					{
						new ChangeSelection
						{
							IsPreview = true,
							Id = t.newPreviewSelection,
						},
						new ChangeSelection
						{
							IsPreview = false,
							Id = t.newSelection,
						},
					}));
		}

		static ObjectIdentifier HitTest(Point<Points> point, ObjectIdentifier currentSelection, PluginContext ctx)
		{
			var hitObjects = (IEnumerable<object>)ctx.Reflection.CallStatic("Outracks.UnoHost.FusionInterop", "HitTest", new Float2((float)point.X, (float)point.Y));

			var objectToId = (IDictionary<object, string>)ctx.Reflection.GetStaticPropertyOrFieldValue(typeof(ObjectTagRegistry).FullName, "ObjectToTag");
			var hitIds = hitObjects
				.SelectMany(o => objectToId.TryGetValue(o))
				.Select(id => new ObjectIdentifier(StringSplitting.BeforeLast(id, "#"), int.Parse(StringSplitting.AfterLast(id, "#"))))
				.ToList();

			return HitTest(hitIds, currentSelection);
		}

		static ObjectIdentifier HitTest(IList<ObjectIdentifier> hitObjects, ObjectIdentifier currentSelection)
		{
			var selectIndex = hitObjects.IndexOf(currentSelection) + 1;

			if (hitObjects.Count == 0)
				return ObjectIdentifier.None;

			return hitObjects
				.Skip(selectIndex % hitObjects.Count)
				.First();
		}
	}
}