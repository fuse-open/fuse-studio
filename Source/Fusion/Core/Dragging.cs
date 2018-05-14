using System;
using System.Reactive;
using System.Reactive.Linq;

namespace Outracks.Fusion
{
	public static class Dragging
	{
		public static IControl OnDragOver(this IControl self, Func<object, bool> canDrop, Action<object> drop, Action<object> enter = null, Action<object> leave = null)
		{
			drop = drop ?? (t => { });
			enter = enter ?? (t => { });
			leave = leave ?? (t => { });
			return Implementation.OnDragOver(self, Observable.Return(canDrop), o => drop(o), o => enter(o), o => leave(o));
		}

		public static IControl OnDragOver<T>(this IControl self, Action<T> drop, Action<T> enter = null, Action<T> leave = null)
		{
			drop = drop ?? (t => { });
			enter = enter ?? (t => { });
			leave = leave ?? (t => { });
			return Implementation.OnDragOver(self, Observable.Return<Func<object, bool>>(o => o is T), o => drop((T)o), o => enter((T)o), o => leave((T)o));
		}

		public static IControl WhileDraggingScrub(
			this IControl control, 
			IProperty<Points> property, 
			Direction2D direction = Direction2D.LeftToRight)
		{
			var unsavedValue = Optional.None<Points>();
			return control.WhileDragging(
				value: property,
				handler: (initial, current) =>
				{
					var newValue =
						direction == Direction2D.LeftToRight ? initial.Value + CalculateDelta(initial.Position.X, current.Position.X) :
						direction == Direction2D.TopToBottom ? initial.Value + CalculateDelta(initial.Position.Y, current.Position.Y) :
						direction == Direction2D.RightToLeft ? initial.Value - CalculateDelta(initial.Position.X, current.Position.X) :
							initial.Value - CalculateDelta(initial.Position.Y, current.Position.Y);	
					
					return Command.Enabled(() =>
					{
						property.Write(newValue, save: false);
						unsavedValue = newValue;
					});
				})
				.OnMouse(released: 
					Command.Enabled(() =>
					{
						if (!unsavedValue.HasValue) return;

						property.Write(unsavedValue.Value, save: true);
						unsavedValue = Optional.None<Points>();
					}))
				.Control;
		}

		static double CalculateDelta(double a, double b)
		{
			return Math.Floor(b) - Math.Floor(a);
		}

		public static Hittable WhileDragging<T>(
			this IControl control,
			IObservable<T> value,
			Func<Positioned<T>, Positioned<T>, Command> handler)
		{
			var hittable = control.MakeHittable(space: Space.Window);

			hittable.Pressed
				.WithLatestFromBuffered(value.ConnectWhile(hittable.Control.IsRooted), Positioned.Create)
				.Switch(pressed =>
					hittable.Moved
						//.Sample(Application.PerFrame)
						.WithLatestFromBuffered(value.ConnectWhile(hittable.Control.IsRooted), Positioned.Create)
						.SelectMany(moved => handler(pressed, moved).Execute.FirstAsync())
						.TakeUntil(hittable.Released.Select(_ => Unit.Default)
							.Merge(hittable.LostFocus)))
				.Subscribe(action => action());

			return hittable;
		}

		public static class Implementation
		{
			public static Func<IControl, IObservable<Func<object, bool>>, Action<object>, Action<object>, Action<object>, IControl> OnDragOver;
		}
	}
}