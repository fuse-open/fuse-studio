using System;
using System.IO;
using System.Reactive;
using System.Reactive.Concurrency;
using System.Reactive.Linq;
using System.Reactive.Subjects;
using System.Text;
using Outracks.Fusion;

namespace Outracks.Fuse.Testing
{
	public static class ConsoleOutputWindow
	{
		static readonly object _initLock = new object();
		static IObservable<string> _toLogView;

		public static void Create()
		{
			var clear = new Subject<Unit>();

			Application.Desktop.CreateSingletonWindow(
				Observable.Return(true),
				dialog => new Window
				{
					Title = Observable.Return("Console output"),
					Size = Property.Create<Optional<Size<Points>>>(new Size<Points>(600, 600)).ToOptional(),
					Content = LogView.Create(InitializeConsoleRedirection(), color: Theme.DefaultText, clear: clear, darkTheme: Theme.IsDark)
								.WithBackground(Theme.PanelBackground)
								.WithOverlay(ThemedButton.Create(Command.Enabled(() => clear.OnNext(Unit.Default)), "Clear").DockTopRight()),
					Background = Theme.PanelBackground,
					Foreground = Theme.DefaultText,
					Border = Separator.MediumStroke
				});
		}

		const int MaxBufferLength = 10000;

		class ObservableTextWriterWrapper : TextWriter, IObservable<string>
		{
			readonly TextWriter _original;
			readonly Subject<string> _subject = new Subject<string>();

			public ObservableTextWriterWrapper(TextWriter original)
			{
				_original = original;
			}

			public override Encoding Encoding
			{
				get { return _original.Encoding; }
			}

			public override void Write(char value)
			{
				_original.Write(value);
				lock (_subject)
				{
					if (_subject.HasObservers)
						_subject.OnNext(value.ToString());
				}
			}

			public override void Write(char[] buffer, int index, int count)
			{
				_original.Write(buffer, index, count);
				lock (_subject)
				{
					if (_subject.HasObservers)
						_subject.OnNext(new string(buffer, index, count));
				}
			}

			public IDisposable Subscribe(IObserver<string> observer)
			{
				lock (_subject)
					return _subject.Subscribe(observer);
			}
		}

		public static IObservable<string> InitializeConsoleRedirection()
		{
			lock (_initLock)
			{
				if (_toLogView == null)
				{
					var @out = new ObservableTextWriterWrapper(Console.Out);
					var error = new ObservableTextWriterWrapper(Console.Error);

					var connectableObservable =
						@out.Merge(error)
							// This may seem odd, but we want to buffer the log through the threadpool to avoid deadlocks
							.ObserveOn(ThreadPoolScheduler.Instance)
							// _then_ dispatch it on the main thread
							.ObserveOn(Application.MainThread)
							.Replay(MaxBufferLength);

					connectableObservable.Connect();

					Console.SetOut(@out);
					Console.SetError(error);
					_toLogView = connectableObservable;
				}
			}
			return _toLogView;
		}
	}
}