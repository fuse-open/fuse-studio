using System;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;

namespace Outracks
{
	public interface IObservableTraceLogger : IDisposable
	{
		int NewObserverId();
		void Log(string str, Action action);
	}

	public static class ObservableTraceLogger
	{
		static IObservableTraceLogger _current;

		/// <summary>
		/// Default <see cref="ObservableTraceLogger"/> for current <see cref="System.Threading.Thread"/>
		/// </summary>
		public static IObservableTraceLogger Current
		{
			get { return _current ?? (_current = new MultiThreadLogger(Console.Out)); }
			set { _current = value; }
		}

		public static IObservableTraceLogger Create(TextWriter writer, bool measure = true, bool lineBufferingEnabled = true, bool disposeWriter = false)
		{
			return new MultiThreadLogger(writer, measure, lineBufferingEnabled, disposeWriter);
		}

		class MultiThreadLogger : IObservableTraceLogger
		{
			int _observerIdCounter;
			readonly ThreadLocal<IObservableTraceLogger> _currentThreadLogger;

			public MultiThreadLogger(TextWriter writer, bool measure = true, bool lineBufferingEnabled = true, bool disposeWriter = false)
			{
				_currentThreadLogger = new ThreadLocal<IObservableTraceLogger>(() => new SingleThreadLogger(writer, measure, lineBufferingEnabled, disposeWriter));
			}

			public int NewObserverId()
			{
				return Interlocked.Increment(ref _observerIdCounter);
			}

			public void Log(string str, Action action)
			{
				_currentThreadLogger.Value.Log(str, action);
			}

			public void Dispose()
			{
				_currentThreadLogger.Dispose();
			}
		}

		class SingleThreadLogger : IObservableTraceLogger
		{

			readonly TextWriter _writer;
			readonly bool _measure;
			readonly bool _lineBufferingEnabled;
			readonly bool _disposeWriter;
			int _logIndentCount;
			int _observerIdCounter;
			bool _pendingLineBreak;
			readonly StringBuilder _lineBuffer = new StringBuilder();

			public SingleThreadLogger(TextWriter writer, bool measure = true, bool lineBufferingEnabled = true, bool disposeWriter = false)
			{
				_writer = writer;
				_measure = measure;
				_lineBufferingEnabled = lineBufferingEnabled;
				_disposeWriter = disposeWriter;
			}

			public int NewObserverId()
			{
				return _observerIdCounter++;
			}

			public void Log(string message, Action action)
			{
				var sw = _measure ? Optional.Some(Stopwatch.StartNew()) : Optional.None();
				var indent = String.Concat(Enumerable.Repeat("|   ", _logIndentCount));
				_logIndentCount++;
				Exception thrownException = null;
				try
				{
					EndLine();
					Append("T{0} Enter  {1}{2}", Thread.CurrentThread.ManagedThreadId, indent, message);
					action();
				}
				catch (Exception ex)
				{
					thrownException = ex;
					throw;
				}
				finally
				{
					var writeExitLine = !_pendingLineBreak;
					if (writeExitLine)
					{
						Append("T{0} Exit   {1}{2}", Thread.CurrentThread.ManagedThreadId, indent, message);
					}
					if (thrownException != null)
					{
						Append(" !!{0}({1})", thrownException.GetType().Name, thrownException.Message);
					}
					if (sw.HasValue)
					{
						var ms = sw.Value.Elapsed.TotalMilliseconds;
						if (ms < 1)
							Append(" {0}μs", ms * 1000.0);
						else
							Append(" {0}ms", ms);
					}
					EndLine();
					_logIndentCount--;
				}
			}

			void Append(string format, params object[] args)
			{
				_pendingLineBreak = true;
				if (_lineBufferingEnabled)
				{
					_lineBuffer.AppendFormat(format, args);
				}
				else
				{
					_writer.Write(format, args);
				}
			}

			void EndLine()
			{
				if (_pendingLineBreak)
				{
					_writer.Write(_lineBuffer);
					_writer.WriteLine();
					_lineBuffer.Clear();
					_pendingLineBreak = false;
				}
			}

			public void Dispose()
			{
				if (_disposeWriter)
					_writer.Dispose();
			}

		}	}
}