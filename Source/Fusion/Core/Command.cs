using System;
using System.Collections.Generic;
using System.Linq;
using System.Reactive.Linq;
using System.Reactive.Threading.Tasks;
using System.Threading.Tasks;

namespace Outracks.Fusion
{
	public static class Commands
	{
		public static Command Then(this Command first, Command next)
		{
			return Command.Create(first.Action.CombineLatest(next.Action, (o1, o2) =>
			{
				return Optional.Combine(o1, o2, (a1, a2) => a1 + a2);
			}));
		}

		public static Command Switch(this IObservable<Command> self)
		{
			return Command.Create(self.Switch(cmd => cmd.Action));
		}

		public static Command Switch<T>(this IObservable<T> self, Func<T, Command> f)
		{
			return Command.Create(self.Switch(t => f(t).Action));
		}

		public static Command SelectMany<T>(this IObservable<T> self, Func<T, Command> f)
		{
			return Command.Create(self.SelectMany(t => f(t).Action));
		}

		public static async Task ExecuteOnceAsync(this Command self)
		{
			(await self.Execute.FirstAsync())();
		}

		public static IObservable<bool> DoExecuteOnce(this Command self)
		{
			return self.Action.Take(1).Do(e => e.Do(f => f())).Select(e => e.HasValue);
		}

		public static IDisposable ExecuteOnce(this Command self)
		{
			return self.Execute.Take(1).Subscribe(e => e());
		}
	}

	public struct Command : IEquatable<Command>
	{
		public static Command Create(bool isEnabled, Action action = null)
		{
			return isEnabled
				? Enabled(action)
				: Disabled;
		}

		public static Command Create(IObservable<bool> isEnabled, Action action = null)
		{
			action = action ?? _noop;
			return Create(isEnabled.Select(b => b ? Optional.Some(action) : Optional.None()));
		}

		public static Command Create(IObservable<bool> isEnabled, IObservable<Action> action)
		{
			return Create(isEnabled.CombineLatest(
				action,
				(enabled, act) => enabled ? Optional.Some(act) : Optional.None()));
		}

		public static Command Create(IObservable<Optional<Action>> action)
		{
			return new Command(action);
		}

		public static Command Enabled(Action action = null)
		{
			return action == null ? _enabledNoop : new Command(Observable.Return(Optional.Some(action)));
		}

		public static Command Enabled(IObservable<Action> action)
		{
			return new Command(action.Select(Optional.Some));
		}

		public override bool Equals(object o)
		{
			return o is Command && Equals((Command)o);
		}

		public bool Equals(Command other)
		{
			return _action == other._action;
		}

		public override int GetHashCode()
		{
			return Action.GetHashCode();
		}

		public static bool operator ==(Command lhs, Command rhs)
		{
			return lhs.Equals(rhs);
		}

		public static bool operator !=(Command lhs, Command rhs)
		{
			return !lhs.Equals(rhs);
		}

		Command(IObservable<Optional<Action>> action)
		{
			_action = action;
		}

		readonly IObservable<Optional<Action>> _action;

		public IObservable<Optional<Action>> Action
		{
			get
			{
				return _action ?? _disabled;
			}
		}

		public IObservable<Action> Execute
		{
			get { return Action.Select(o => o.Or(_noop)); }
		}

		public IObservable<bool> IsEnabled
		{
			get { return Action.Select(o => o.HasValue); }
		}

		static Command()
		{
			_noop = () => { };
			_enabledNoop = Create(Observable.Return(Optional.Some(_noop)));
			_disabled = Observable.Return(Optional.None<Action>());
		}

		public static Command Disabled
		{
			get { return new Command(); }
		}

		static readonly Action _noop;
		static readonly Command _enabledNoop;
		static readonly IObservable<Optional<Action>> _disabled;
	}
}