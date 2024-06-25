using System;
using System.IO;
using System.Reactive.Subjects;
using System.Text;
using Outracks.Fuse.Protocol;
using Outracks.IO;
using Uno;
using Uno.Logging;

namespace Fuse.Preview
{
	class ReifyerLogAdapter
	{
		readonly StringBuilderProgress _progress;

		readonly Log _log;
		public Log Log { get { return _log; } }

		readonly Subject<IEventData> _events = new Subject<IEventData>();
		public IObservable<IEventData> Events { get { return _events; } }

		public ReifyerLogAdapter()
		{
			_progress = new StringBuilderProgress();
			var errorList = new StringProgressErrorListAdapter(_progress);
			var textWriter = new StringProgressTextWriterAdapter(_progress);
			_log = new Log(errorList, textWriter);
		}

		public void Success(AbsoluteFilePath path)
		{
			_events.OnNext(new ReifyFileSuccessEvent()
			{
				Path = path.NativePath,
				Message = _progress.Flush()
			});
		}

		public void Error(AbsoluteFilePath path)
		{
			_events.OnNext(new ReifyFileErrorEvent()
			{
				Path = path.NativePath,
				Message = _progress.Flush()
			});
		}
	}

	class StringBuilderProgress : IProgress<string>
	{
		readonly StringBuilder _sb = new StringBuilder();

		public void Report(string value)
		{
			_sb.Append(value);
		}

		public string Flush()
		{
			var ret = _sb.ToString();

			_sb.Clear();

			return ret;
		}
	}

	// TODO: Consider not throwing away additional semantics
	class StringProgressErrorListAdapter : IErrorList
	{
		readonly IProgress<string> _progress;

		public StringProgressErrorListAdapter(IProgress<string> progress)
		{
			_progress = progress;
		}

		public void AddError(Source src, string code, string msg)
		{
			_progress.Report("Error: " + msg);
		}

		public void AddFatalError(Source src, string code, string msg)
		{
			_progress.Report("Fatal error: " + msg);
		}

		public void AddMessage(Source src, string code, string msg)
		{
			_progress.Report(msg);
		}

		public void AddWarning(Source src, string code, string msg)
		{
			_progress.Report("Warning: " + msg);
		}
	}

	class StringProgressTextWriterAdapter : TextWriter
	{
		readonly IProgress<string> _progress;

		public StringProgressTextWriterAdapter(IProgress<string> progress)
		{
			_progress = progress;
		}

		public override void Write(char value)
		{
			Write(new string(value, 1));
		}

		public override void WriteLine(string value)
		{
			Write(value + "\n");
		}

		public override void WriteLine()
		{
			Write("\n");
		}

		public override void Write(string value)
		{
			_progress.Report(value);
		}

		public override Encoding Encoding
		{
			get { return Encoding.UTF8; }
		}
	}
}