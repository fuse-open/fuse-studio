using System;
using System.Reactive.Linq;
using System.Reactive.Subjects;

namespace Outracks.Fusion
{
	public static class Animation
	{
		

		public static Size<IObservable<Points>> LowPass(
			this Size<IObservable<Points>> size, 
			double amount = 0.5,
			double tolerance = double.Epsilon)
		{
			return size
				.Select(component => component
					.Select(p => p.ToDouble())
					.LowPass(amount, tolerance)
					.Select(d => new Points(d)))
				.Replay(1).RefCount();
		}

		public static IObservable<Points> LowPass(
			this IObservable<Points> targets,
			double amount = 0.5,
			double tolerance = double.Epsilon)
		{
			return targets
				.Select(t => t.ToDouble())
				.LowPass(amount, tolerance)
				.Select(d => new Points(d))
				.Replay(1).RefCount();
		}

		public static IObservable<double> LowPass(
			this IObservable<double> targets,
			double amount = 0.5,
			double tolerance = double.Epsilon)
		{
			return targets.Animate((from, to) => 
				Application
					.PerFrame
					.Scan(from, (current, frame) => to * (1 - amount) + current * amount)
					.TakeWhile(result =>  Math.Abs(result - to) > tolerance)
					.Concat(Observable.Return(to)));
		}

		public static IObservable<double> QuintEaseOut(
			this IObservable<double> targets,
			TimeSpan duration)
		{
			const double fps = 60.0;
			var durationInFrames = (int)(duration.TotalSeconds * fps);

			return targets.Animate((from, to) =>
			{
				var easingFunc = QuintEaseOut(durationInFrames, from, to);
				return Application
					.PerFrame
					.Select((i,j) => (long)j)
					.Take(durationInFrames)
					.Select(easingFunc)
					.Concat(Observable.Return(to));
			});
		}

		static Func<long, double> QuintEaseOut(long duration, double from, double to)
		{
			return FlipArgumentsIfNeeded(OneWayQuintEaseOut, duration, from, to);
		}

		static Func<long, double> OneWayQuintEaseOut(long duration, double from, double to)
		{
			return ti =>
			{
				var t = (double)ti;
				var d = (double)duration;
				var b = from;
				var c = to;
				return c * ((t = t / d - 1) * t * t * t * t + 1) + b;
			};
		}

		/// The easing functions you find online only works in one direction, so if you want to copy paste them you might want to call it through this thing
		static Func<long, double> FlipArgumentsIfNeeded(Func<long, double, double, Func<long, double>> easingFuncFactory, long duration, double from, double to)
		{
			if (to < from)
			{
				return ti => from - easingFuncFactory(duration, to, from)(ti);
			}

			return easingFuncFactory(duration, from, to);
		}

		public static IObservable<double> Animate(
			this IObservable<double> destinations,
			Func<double, double, IObservable<double>> animate)
		{
			return Observable
				.Create<double>(observer =>
				{
					var results = new BehaviorSubject<double>(double.NaN);

					return Disposable.Combine(
						results,

						results.Skip(1).Subscribe(observer),

						destinations.SubscribeUsing(destination =>
						{
							var source = results.Value;

							if (double.IsNaN(source))
							{
								// No previous value, jump to destination 
								results.OnNext(destination);
								return Disposable.Empty;
							}

							// we have a source and a destination, let's animate between them
							// and emit the results (until we're disposed by a new destination)
							return animate(source, destination).Subscribe(results.OnNext);
						}));
				})
				.Replay(1).RefCount();
		}
	}
}