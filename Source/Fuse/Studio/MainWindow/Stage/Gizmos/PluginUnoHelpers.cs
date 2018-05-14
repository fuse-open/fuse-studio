using System;
using System.Collections.Generic;
using System.Reactive.Linq;

namespace Outracks.Fuse.Stage
{
	using Simulator;
	using Simulator.Runtime;
	using UnoHost;

	static class PluginUnoHelpers
	{
		public static IObservable<Thickness<Points>> GetThickness(this PluginContext context, IObservable<object> observableUnoObject, string property)
		{
			return context.ObservePerFrame(observableUnoObject, obj =>
			{
				var vector = context.Reflection.GetPropertyValue(obj, property) as dynamic;
				float l = vector.X;
				float t = vector.Y;
				float r = vector.Z;
				float b = vector.W;
				return Thickness.Create<Points>(l, t, r, b);
			})
				.Catch((Exception e) =>
				{
					Console.WriteLine(e);
					return Observable.Return(Thickness.Create<Points>(0, 0, 0, 0));
				})
				.DistinctUntilChanged()
				.Replay(1).RefCount();
		}

		public static IObservable<Rectangle<Points>> GetBounds(this PluginContext context, IObservable<object> observableUnoObject)
		{
			return context.ObservePerFrame(observableUnoObject, obj =>
			{
				float x = obj.WorldPosition.X + obj.LocalBounds.Minimum.X;
				float y = obj.WorldPosition.Y + obj.LocalBounds.Minimum.Y;
				float w = obj.LocalBounds.Maximum.X - obj.LocalBounds.Minimum.X;
				float h = obj.LocalBounds.Maximum.Y - obj.LocalBounds.Minimum.Y;
				return Rectangle.FromPositionSize(
					Point.Create<Points>(x, y),
					Size.Create<Points>(w, h));
			})
				.Catch((Exception e) =>
				{
					Console.WriteLine(e);
					return Observable.Return(Rectangle.FromPositionSize<Points>(0, 0, 0, 0));
				})
				.DistinctUntilChanged()
				.Replay(1).RefCount();
		}

		static IObservable<T> ObservePerFrame<T>(this PluginContext context, IObservable<object> unoObject, Func<dynamic, T> readValue)
		{
			return context.PerFrame.StartWith(0).CombineLatest(unoObject, (_, v) => readValue(v));
		}

		public static IObservable<IEnumerable<object>> GetObjects(this PluginContext context, ObjectIdentifier id)
		{
			return context.PerFrame
				.StartWith(0)
				.Select(_ =>
					(IEnumerable<object>)context.Reflection.CallStatic(typeof(ObjectTagRegistry).FullName, "GetObjectsWithTag", id.ToString()));
		}
	}
}