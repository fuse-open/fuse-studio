using System;
using Uno.Collections;
using Uno;
using Uno.Platform;

namespace Outracks.Simulator
{
	public static class Closure
	{
		public static Action<TArg2> ApplyFirst<TArg1, TArg2>(Action<TArg1, TArg2> action, TArg1 arg1)
		{
			return new ApplyFirst<TArg1, TArg2>(action, arg1).Execute;
		}

		public static Action<TArg1> ApplySecond<TArg1, TArg2>(Action<TArg1, TArg2> action, TArg2 arg2)
		{
			return new ApplySecond<TArg1, TArg2>(action, arg2).Execute;
		}

		public static Func<TArg, TResult> Return<TArg, TResult>(TResult result)
		{
			return new Forget<TArg, TResult>(result).Execute;
		}

		public static Action Apply<T>(this Action<T> action, T arg)
		{
			return new Apply<T>(action, arg).Execute;
		}

		public static Action<TArg1, TArg2> Forget<TArg1, TArg2>(this Action action)
		{
			return new ForgetAction<TArg1, TArg2>(action).Execute;
		}

		public static EventHandler ToEventHandler(this Action action)
		{
			return new ForgetAction<object, EventArgs>(action).Execute;
		}

		public static ApplicationStateTransitionHandler ToAppStateChangeHandler(this Action action)
		{
			return new ForgetAction<ApplicationState>(action).Execute;
		}
	}


	class Forget<TArg, TResult>
	{
		readonly TResult _result;
		public Forget(TResult result)
		{
			_result = result;
		}

		public TResult Execute(TArg _)
		{
			return _result;
		}
	}

	public class ForgetAction<TArg1>
	{
		readonly Action _action;

		public ForgetAction(Action action)
		{
			_action = action;
		}

		public void Execute(TArg1 arg1)
		{
			_action();
		}
	}


	public class ForgetAction<TArg1, TArg2>
	{
		readonly Action _action;

		public ForgetAction(Action action)
		{
			_action = action;
		}

		public void Execute(TArg1 arg1, TArg2 arg2)
		{
			_action();
		}
	}

	class Forget<TArg1, TArg2, TResult>
	{
		readonly TResult _result;
		public Forget(TResult result)
		{
			_result = result;
		}

		public TResult Execute(TArg1 arg1, TArg2 arg2)
		{
			return _result;
		}
	}

	class Apply<TArg1>
	{
		readonly Action<TArg1> _action;
		readonly TArg1 _arg1;

		public Apply(Action<TArg1> action, TArg1 arg1)
		{
			_action = action;
			_arg1 = arg1;
		}

		public void Execute()
		{
			_action(_arg1);
		}
	}

	class ApplyFirst<TArg1, TArg2>
	{
		readonly Action<TArg1, TArg2> _action;
		readonly TArg1 _arg1;
		public ApplyFirst(Action<TArg1, TArg2> action, TArg1 arg1)
		{
			_action = action;
			_arg1 = arg1;
		}

		public void Execute(TArg2 arg2)
		{
			_action(_arg1, arg2);
		}
	}

	class ApplySecond<TArg1, TArg2>
	{
		readonly Action<TArg1, TArg2> _action;
		readonly TArg2 _arg2;
		public ApplySecond(Action<TArg1, TArg2> action, TArg2 arg2)
		{
			_action = action;
			_arg2 = arg2;
		}

		public void Execute(TArg1 arg1)
		{
			_action(arg1, _arg2);
		}
	}

}
