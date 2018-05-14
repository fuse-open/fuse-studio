using System;
using System.Reactive.Linq;

namespace Outracks.Fusion
{
	/// <summary>
	/// IMountLocation is an observable data structure that defines the information passed from parent to children in the control hierarchy. 
	/// The information passed in the opposite direction, from child to parent, is exposed as part of IControl.
	/// </summary>
	public interface IMountLocation
	{
		/// <summary>
		/// Whether this control is currently part of the control hierarchy of a visible window.
		/// This can be used to connect subscriptions and manage other resources to avoid leaks when a control is either removed from its parent or the window is hidden.
		/// </summary>
		IObservable<bool> IsRooted { get; }

		/// <summary>
		/// The size should only be used as an aid in calculating the DesiredSize of the IControl
		/// It depends on the parent control's layout rules whether the NativeFrame of the IControl ends up being this size.
		/// </summary>
		Size<IObservable<Points>> AvailableSize { get; }

		/// <summary>
		/// The actual size and position of the native control placed in its parent container control.
		/// Do not use this to calculate the DesiredSize of the IControl, as that can cause feedback loops, unstable layout or just nothing showing up.
		/// </summary>
		Rectangle<IObservable<Points>> NativeFrame { get; }
	}

	public static class MountLocation
	{
		public class Mutable : IMountLocation
		{
			public IObservable<bool> IsRooted { get; set; }

			public Size<IObservable<Points>> AvailableSize { get; set; }

			public Rectangle<IObservable<Points>> NativeFrame { get; set; }
		}

		public static readonly IMountLocation Unmounted = new Mutable
		{
			IsRooted = Observable.Return(false),
			AvailableSize = ObservableMath.NeverSize,
			NativeFrame = ObservableMath.NeverRectangle,
		};

		public static IMountLocation Switch(this IObservable<IMountLocation> self)
		{
			return new Mutable
			{
				IsRooted = self
					.Select(s => s.IsRooted).Switch()
					.DistinctUntilChanged()
					.Replay(1).RefCount(),
				
				AvailableSize = self
					.Select(s => s.AvailableSize).Switch()
					.DistinctUntilChanged()
					.Replay(1).RefCount(),
				
				NativeFrame = self
					.Select(s => s.NativeFrame).Switch()
					.DistinctUntilChanged()
					.Replay(1).RefCount(),	
			};
		}
	}
}
