using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using System.Reactive.Linq;


namespace Outracks.Fusion
{
	public static class Overlay
	{

		public static IControl WithBackground(this IControl control, Brush background)
		{
			return control.WithBackground(Shapes.Rectangle(fill: background));
		}

		public static IControl WithBackground(this IControl foreground, IControl background)
		{
			return Layout
				.Layer(background, foreground)
				.WithSize(foreground.DesiredSize);
		}

		public static IControl WithOverlay(this IControl background, IControl foreground)
		{
			return Layout
				.Layer(background, foreground)
				.WithSize(background.DesiredSize);
		}

		static Func<IControl, IControl, IControl> _factory;

		public static void Initialize(Func<IControl, IControl, IControl> factory)
		{
			_factory = factory;
		}

		public static IControl WithNativeOverlay(this IControl background, IControl foreground)
		{
			return (_factory ?? WithOverlay)(background, foreground);
		}
	}

	public static partial class Layout
	{

		public static IControl Layer(params IControl[] layers)
		{
			return Layer((IEnumerable<IControl>) layers);
		}

		public static IControl Layer(this IEnumerable<IControl> layers)
		{
			return Implementation.LayerControls(_ => Observable.Return(layers.ToImmutableList()));
		}

		public static IControl Layer(this IObservable<IEnumerable<IControl>> layers)
		{
			return Implementation.LayerControls(_ => layers.Select(ImmutableList.ToImmutableList));
		}

		public static IControl Layer(this Func<IMountLocation, IObservable<IEnumerable<IControl>>> layers)
		{
			return Implementation.LayerControls(self => layers(self).Select(ImmutableList.ToImmutableList));
		}

		public static IControl Layer(Func<IMountLocation, IEnumerable<IControl>> layers)
		{
			return Implementation.LayerControls(self => Observable.Return(layers(self).ToImmutableList()));
		}

		public static IControl Layer(Func<IObservable<bool>, Size<IObservable<Points>>, Rectangle<IObservable<Points>>, IEnumerable<IControl>> layers)
		{
			return Implementation.LayerControls(self => Observable.Return(layers(self.IsRooted, self.AvailableSize, self.NativeFrame).ToImmutableList()));
		}


		public static class Implementation
		{
			public static Func<Func<IMountLocation,  IObservable<IImmutableList<IControl>>>, IControl> LayerControls;

			static Implementation()
			{
				LayerControls = layers => Control.Create(self =>
				{

					layers(self).DiffSequence().Subscribe(
						diff =>
						{
							foreach (var child in diff.Removed.Reverse())
							{
								child.Mount(MountLocation.Unmounted);
							}

							foreach (var child in diff.Added)
							{
								child.Mount(self);

								var lol = child.NativeHandle;
							}
						});

					return null;
				});

			}

		}

	}
}