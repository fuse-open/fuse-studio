using System;
using System.Collections;
using System.Linq;
using System.Reactive.Subjects;

namespace Outracks
{
	public static class ConsoleTraceExtensions
	{
		/// <summary>
		/// Logs activity of <paramref name="source"/> to stdout.
		/// </summary>
		/// <param name="source">The <see cref="IObservable{T}"/> to trace</param>
		/// <param name="name">Optional name of observable, to make it easier to locate in trace</param>
		/// <param name="toString">Optional delegate to call when converting to string, to override <see cref="object.ToString()"/></param>
		public static IObservable<T> ConsoleTrace<T>(
			this IObservable<T> source,
			string name = "(anonymous)",
			Func<T, string> toString = null)
		{
			toString = toString ?? (x => DefaultToStringShortened(x));
			return new TraceOperator<T>(source, name, ObservableTraceLogger.Current, toString);
		}

		/// <summary>
		/// Logs activity of <paramref name="source"/> to stdout.
		/// </summary>
		/// <param name="source">The <see cref="IConnectableObservable{T}"/> to trace</param>
		/// <param name="name">Optional name of observable, to make it easier to locate in trace</param>
		/// <param name="toString">Optional delegate to call when converting to string, to override <see cref="object.ToString()"/></param>
		public static IConnectableObservable<T> ConsoleTrace<T>(
			this IConnectableObservable<T> source,
			string name = "(anonymous)",
			Func<T, string> toString = null)
		{
			toString = toString ?? (x => DefaultToStringShortened(x));
			return new ConnectableTraceOperator<T>(source, name, ObservableTraceLogger.Current, toString);
		}

		/// <summary>
		/// Logs activity of <paramref name="subject"/> to stdout.
		/// </summary>
		/// <param name="subject">The <see cref="ISubject{T}"/> to trace</param>
		/// <param name="name">Optional name of observable, to make it easier to locate in trace</param>
		/// <param name="toString">Optional delegate to call when converting to string, to override <see cref="object.ToString()"/></param>
		public static ISubject<T> ConsoleTrace<T>(
			this ISubject<T> subject,
			string name = "(anonymous)",
			Func<T, string> toString = null)
		{
			toString = toString ?? (x => DefaultToStringShortened(x));
			return new SubjectTraceOperator<T>(subject, name, ObservableTraceLogger.Current, toString);
		}

		static string DefaultToStringShortened(object item)
		{
			var str = DefaultToString(item);
			if (str.Length > 40)
				return str.Substring(0, 40) + "..";
			return str;
		}

		static string DefaultToString(object item)
		{
			if (item != null)
			{
				var type = item.GetType();
				if (type == typeof(string))
				{
					// Surround with quotes to make recognizable as strings. Don't care about escaping for now.
					return string.Format("\"{0}\"", item);
				}
				if (type.IsArray)
				{
					return "[" + string.Join(",", ((IEnumerable) item).Cast<object>().Select(DefaultToString)) + "]";
				}
				return item.ToString();
			}
			return "(null)";
		}

		class ConnectableTraceOperator<T> : TraceOperator<T>, IConnectableObservable<T>
		{
			public ConnectableTraceOperator(
				IConnectableObservable<T> source,
				string name,
				IObservableTraceLogger logger,
				Func<T, string> itemToString) :
				base(source, name, logger, itemToString) { }

			public IDisposable Connect()
			{
				IDisposable wrappedDisposer = null;
				LoggedInvoke(
					"Connect()",
					() => wrappedDisposer = new ConnectDisposer(((IConnectableObservable<T>) Source).Connect(), this));
				return wrappedDisposer;
			}

			class ConnectDisposer : IDisposable
			{
				readonly IDisposable _inner;
				readonly ConnectableTraceOperator<T> _parent;

				public ConnectDisposer(IDisposable inner, ConnectableTraceOperator<T> parent)
				{
					_inner = inner;
					_parent = parent;
				}

				public void Dispose()
				{
					_parent.LoggedInvoke("Dispose (disconnect)", () => _inner.Dispose());
				}
			}
		}

		class SubjectTraceOperator<T> : TraceOperator<T>, ISubject<T>
		{
			readonly ISubject<T> _subject;
			public SubjectTraceOperator(ISubject<T> subject, string name, IObservableTraceLogger logger, Func<T, string> itemToString)
				: base(subject, name, logger, itemToString)
			{
				_subject = subject;
			}

			public void OnNext(T value)
			{
				LoggedInvoke(string.Format("{0}({1})", "OnNext", _itemToString(value)), () => _subject.OnNext(value));
			}

			public void OnError(Exception error)
			{
				LoggedInvoke(string.Format("{0}({1})", "OnError", error.GetType()), () => _subject.OnError(error));
			}

			public void OnCompleted()
			{
				LoggedInvoke(string.Format("{0}()", "OnCompleted"), () => _subject.OnCompleted());
			}

		}

		class TraceOperator<T> : IObservable<T>
		{
			readonly IObservableTraceLogger _logger;
			protected  readonly Func<T, string> _itemToString;
			readonly string _name;
			readonly IObservable<T> _source;

			protected IObservable<T> Source
			{
				get { return _source; }
			}

			public TraceOperator(IObservable<T> source, string name, IObservableTraceLogger logger, Func<T, string> itemToString)
			{
				_source = source;
				_name = name;
				_logger = logger;
				_itemToString = itemToString;
			}

			public IDisposable Subscribe(IObserver<T> observer)
			{
				IDisposable wrappedDisposer = null;
				var id = _logger.NewObserverId();
				LoggedInvoke(
					string.Format("{0}(..) -> #{1}", "Subscribe", id),
					() =>
					{
						wrappedDisposer = new TraceDisposer(this, _source.Subscribe(new TraceObserver(this, observer, id)), id);
					});
				return wrappedDisposer;
			}

			protected void LoggedInvoke(string message, Action action)
			{
				_logger.Log(string.Format("{0} {1}", _name, message), action);
			}

			class TraceObserver : IObserver<T>
			{
				readonly int _id;
				readonly IObserver<T> _inner;
				readonly TraceOperator<T> _parent;

				public TraceObserver(TraceOperator<T> parent, IObserver<T> inner, int id)
				{
					_parent = parent;
					_inner = inner;
					_id = id;
				}

				public void OnNext(T value)
				{
					LoggedInvoke(string.Format("{0}({1})", "OnNext", _parent._itemToString(value)), () => _inner.OnNext(value));
				}

				public void OnError(Exception error)
				{
					LoggedInvoke(string.Format("{0}({1})", "OnError", error.GetType()), () => _inner.OnError(error));
				}

				public void OnCompleted()
				{
					LoggedInvoke(string.Format("{0}()", "OnCompleted"), () => _inner.OnCompleted());
				}

				void LoggedInvoke(string message, Action action)
				{
					_parent.LoggedInvoke(string.Format("(#{0}) {1}", _id, message), action);
				}
			}

			class TraceDisposer : IDisposable
			{
				readonly int _id;
				readonly IDisposable _inner;
				readonly TraceOperator<T> _parent;

				public TraceDisposer(TraceOperator<T> parent, IDisposable inner, int id)
				{
					_parent = parent;
					_inner = inner;
					_id = id;
				}


				public void Dispose()
				{
					_parent.LoggedInvoke(string.Format("(#{0}) {1}()", _id, "Dispose"), () => _inner.Dispose());
				}
			}
		}
	}
}