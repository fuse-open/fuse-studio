using System;
using System.Reactive.Concurrency;
using System.Reactive.Linq;
using System.Reactive;
using System.Reactive.Subjects;

namespace Outracks.Fusion
{
	public enum Space
	{
		Local,
		Window,
	}

	public static class Pointer
	{
		public static Hittable MakeHittable(this IControl control, IObservable<object> dragData = null, Space space = Space.Local)
		{
			dragData = dragData ?? Observable.Empty<object>();

			var latestData = Optional.None<object>();
			dragData.ConnectWhile(control.IsRooted)
				.Subscribe(d => latestData = d);

			var onPressed = new Subject<OnPressedArgs>();
			var released = new Subject<Unit>();
			var entered = new Subject<Point<Points>>();
			var exited = new Subject<Point<Points>>();
			var moved = new Subject<Point<Points>>();
			var gotFocus = new Subject<Unit>();
			var lostFocus = new Subject<Unit>();

			var callbacks = new Callbacks()
			{
				OnPressed = onPressed.OnNext,
				OnReleased = () => released.OnNext(Unit.Default),
				OnEntered = entered.OnNext,
				OnExited = exited.OnNext,
				OnMoved = moved.OnNext,
				OnDragged = () => latestData,
				OnGotFocus = () => gotFocus.OnNext(Unit.Default),
				OnLostFocus = () => lostFocus.OnNext(Unit.Default),
			};

			var hittableControl = Implementation.MakeHittable(control, space, callbacks);

			var pressed = onPressed.Publish().RefCount();

			return new Hittable()
			{
				Control = hittableControl,
				Pressed = pressed.Select(a => a.Position),
				Released = released.Publish().RefCount(),
				DoubleClicked = pressed.SelectSome(a => a.ClickCount == 2 ? Optional.Some(a.Position) : Optional.None()),
				Entered = entered.Publish().RefCount(),
				Exited = exited.Publish().RefCount(),
				Moved = moved.Publish().RefCount(),
				GotFocus = gotFocus.Publish().RefCount(),
				LostFocus = lostFocus.Publish().RefCount(),
			};
		}

		public static Hittable OnMouse(
			this Hittable hittable,
			Optional<Command> pressed = default(Optional<Command>),
			Optional<Command> released = default(Optional<Command>),
			Optional<Command> doubleClicked = default(Optional<Command>),
			Optional<Command> entered = default(Optional<Command>),
			Optional<Command> exited = default(Optional<Command>))
		{
			ExecCommandWhileRooted(hittable.Pressed, pressed, hittable.Control);
			ExecCommandWhileRooted(hittable.Released, released, hittable.Control);
			ExecCommandWhileRooted(hittable.DoubleClicked, doubleClicked, hittable.Control);
			ExecCommandWhileRooted(hittable.Entered, entered, hittable.Control);
			ExecCommandWhileRooted(hittable.Exited, exited, hittable.Control);

			return hittable;
		}

		public static IControl OnMouse(
			this IControl control,
			Optional<Command> pressed = default(Optional<Command>),
			Optional<Command> released = default(Optional<Command>),
			Optional<Command> doubleClicked = default(Optional<Command>),
			Optional<Command> entered = default(Optional<Command>),
			Optional<Command> exited = default(Optional<Command>),
			IObservable<object> dragged = null)
		{
			var hittableControl = control.MakeHittable(dragData: dragged);
			return hittableControl.OnMouse(pressed, released, doubleClicked, entered, exited).Control;
		}

		static void ExecCommandWhileRooted<T>(IObservable<T> obs, Optional<Command> command, IControl control)
		{
			if (command.HasValue)
				obs
					.WithLatestFromBuffered(command.Value.Execute.ConnectWhile(control.IsRooted), (_, c) => c)
					.Subscribe(c => c());
		}

		public static IObservable<bool> IsPressed(this Hittable control)
		{
			return Observable
				.Merge(
					control.Pressed.Select(_ => true),
					control.Released.Select(_ => false),
					control.LostFocus.Select(_ => false))
				.StartWith(false);
		}

		public static IObservable<bool> IsHovering(this Hittable control)
		{
			return Observable
				.Merge(
					control.Entered.Select(_ => true),
					control.Exited.Select(_ => false))
				.StartWith(false);
		}

		public struct Callbacks
		{
			public Action<OnPressedArgs> OnPressed;
			public Action OnReleased;
			public Action<Point<Points>> OnEntered;
			public Action<Point<Points>> OnExited;
			public Action<Point<Points>> OnMoved;
			public Func<Optional<object>> OnDragged;
			public Action OnGotFocus;
			public Action OnLostFocus;
		}

		public struct OnPressedArgs
		{
			public readonly Point<Points> Position;
			public readonly int ClickCount;

			public OnPressedArgs(Point<Points> position, int clickCount)
			{
				Position = position;
				ClickCount = clickCount;
			}
		}

		public static class Implementation
		{
			public static Func<IControl, Space, Callbacks, IControl> MakeHittable;
		}
	}

	public struct Hittable
	{
		public IControl Control;
		public IObservable<Point<Points>> Pressed;
		public IObservable<Unit> Released;
		public IObservable<Point<Points>> DoubleClicked;
		public IObservable<Point<Points>> Entered;
		public IObservable<Point<Points>> Exited;
		public IObservable<Point<Points>> Moved;
		public IObservable<Unit> GotFocus;
		public IObservable<Unit> LostFocus;
	}
}