using System;

namespace Outracks.Fusion
{
	public static class ConsoleTraceExtensions
	{
		/// <summary>
		/// Logs activity of observables contained in <paramref name="source"/> to stdout.
		/// </summary>
		/// <param name="source">The <see cref="Rectangle"/> to trace</param>
		/// <param name="name">Optional name of observable, to make it easier to locate in trace</param>
		/// <param name="toString">Optional delegate to call when converting to string, to override <see cref="object.ToString()"/></param>
		public static Size<IObservable<T>> ConsoleTrace<T>(
			this Size<IObservable<T>> source,
			string name = "(anonymous size)",
			Func<T, string> toString = null)
		{
			return Size.Create(
				source.Width.ConsoleTrace(name + ".Width", toString),
				source.Height.ConsoleTrace(name + ".Height", toString));
		}

		/// <summary>
		/// Logs activity of observables contained in <paramref name="source"/> to stdout.
		/// </summary>
		/// <param name="source">The <see cref="Rectangle"/> to trace</param>
		/// <param name="name">Optional name prefix for observables, to make it easier to locate in trace</param>
		/// <param name="toString">Optional delegate to call when converting to string, to override <see cref="object.ToString()"/></param>
		public static Interval<IObservable<T>> ConsoleTrace<T>(
			this Interval<IObservable<T>> source,
			string name = "(anonymous interval)",
			Func<T, string> toString = null)
		{
			return Interval.FromOffsetLength(
				source.Offset.ConsoleTrace(name + ".Offset", toString),
				source.Length.ConsoleTrace(name + ".Length", toString));
		}

		/// <summary>
		/// Logs activity of observables contained in <paramref name="source"/> to stdout.
		/// </summary>
		/// <param name="source">The <see cref="Rectangle"/> to trace</param>
		/// <param name="name">Optional name prefix for observables, to make it easier to locate in trace</param>
		/// <param name="toString">Optional delegate to call when converting to string, to override <see cref="object.ToString()"/></param>
		public static Rectangle<IObservable<T>> ConsoleTrace<T>(
			this Rectangle<IObservable<T>> source,
			string name = "(anonymous rectangle)",
			Func<T, string> toString = null)
		{
			return Rectangle.FromPositionSize(
				source.Left().ConsoleTrace(name + ".Left", toString),
				source.Top().ConsoleTrace(name + ".Top", toString),
				source.Width.ConsoleTrace(name + ".Width", toString),
				source.Height.ConsoleTrace(name + ".Height", toString));
		}


		/// <summary>
		/// Logs activity of observables contained in <paramref name="source"/> to stdout.
		/// </summary>
		/// <param name="source">The <see cref="Rectangle"/> to trace</param>
		/// <param name="name">Optional name prefix for observables, to make it easier to locate in trace</param>
		public static IControl ConsoleTrace(this IControl source, string name = "(anonymous control)")
		{
			return new TraceControl(source, name);
		}

		/// <summary>
		/// Logs activity of observables contained in <paramref name="source"/> to stdout.
		/// </summary>
		/// <param name="source">The <see cref="Rectangle"/> to trace</param>
		/// <param name="name">Optional name prefix for observables, to make it easier to locate in trace</param>
		public static IMountLocation ConsoleTrace(this IMountLocation source, string name = "(anonymous mount location)")
		{
			return new MountLocation.Mutable
			{
				IsRooted = source.IsRooted.ConsoleTrace(name + ".IsRooted"),
				AvailableSize = source.AvailableSize.ConsoleTrace(name + ".AvailableSize"),
				NativeFrame = source.NativeFrame.ConsoleTrace(name + ".NativeFrame")
			};
		}
	}

	class TraceControl : IControl
	{
		readonly IControl _source;

		public TraceControl(IControl source, string name )
		{
			_source = source;
			var mount = source.Mount;
			DesiredSize = source.DesiredSize.ConsoleTrace(name + ".DesiredSize");
			IsRooted = source.IsRooted.ConsoleTrace(name + ".IsRooted");
			Mount = mountLocation => mount(mountLocation.ConsoleTrace(name + ".MountLocation"));
		}

		public object NativeHandle
		{
			get { return _source.NativeHandle; }
		}

		public Action<IMountLocation> Mount { get; private set; }

		public Size<IObservable<Points>> DesiredSize { get; private set; }

		public IObservable<bool> IsRooted { get; private set; }
	}
}