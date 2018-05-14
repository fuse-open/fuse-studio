using System;
using System.Reactive.Subjects;

namespace Outracks.Fusion
{
	
	/// <summary>
	/// A control represents a rectangular area on the screen where user interaction happens.
	/// Controls are typically composed in a tree, where each parent control fully encapsulate its children.
	/// All non-empty controls can typically only have one parent at any given time.
	/// 
	/// This interface is meant for consuption by the direct parent/owner of the control it encapsulates, 
	/// all control-specific parameters and callbacks should be passed in to the control during creation.
	/// </summary>
	/// <remarks>
	/// IControl implementations should _only_ observe their respective external data sources while rooted. 
	/// Failure to do this results in memory leaks and bad performance over time because the data sources are 
	/// updating unreachable controls that are only kept alive by said subscriptions. 
	/// The data sources always have equal or longer life-time than the controls observing them.
	/// </remarks>
	public interface IControl 
	{
		/// <summary>
		/// The act of making a control part of a bigger hierarchy is called "mounting". 
		/// When a control is part of a window's control hierarchy it is said to be "rooted". 
		///
		/// The parent control is responsible for invoking the Mount delegate of its children, passing down the 
		/// information needed for them to do local layout and knowing when they are rooted (for subscription management)
		/// </summary>
		Action<IMountLocation> Mount { get; }

		/// <summary>
		/// Desired size is expressed in terms of available size, passed to Mount as part of IMountLocation.
		/// </summary>
		Size<IObservable<Points>> DesiredSize { get; }
	
		/// <summary>
		/// The underlying native control where you'll find the contents of this IControl
		/// Needs to be accessed on the main thread
		/// </summary>
		object NativeHandle { get; }

		IObservable<bool> IsRooted { get; } 
	}

	public static partial class Control
	{
		public static IControl Create(Func<IMountLocation, object> factory)
		{
			return new NativeControl(factory);
		}
	}

	class NativeControl : IControl
	{
		readonly ISubject<IMountLocation> _mountLocation = new ReplaySubject<IMountLocation>(1);
		readonly IMountLocation _currentMountLocation;
		readonly Func<IMountLocation, object> _factory;

		object _nativeHandle;

		public NativeControl(Func<IMountLocation, object> factory)
		{
			_factory = factory;
			_currentMountLocation = _mountLocation.Switch();
		}

		public Action<IMountLocation> Mount
		{
			get { return _mountLocation.OnNext; }
		}

		public Size<IObservable<Points>> DesiredSize
		{
			get { return ObservableMath.ZeroSize; }
		}

		public object NativeHandle
		{
			get { return _nativeHandle ?? (_nativeHandle = _factory(_currentMountLocation)); }
		}

		public IObservable<bool> IsRooted
		{
			get { return _currentMountLocation.IsRooted; }
		}
	}

}
