using System;
using System.Linq;
using System.Reactive.Linq;
using System.Reactive.Subjects;
using Outracks.Fusion;
using Outracks.IO;

namespace Outracks.Fuse.Testing
{
	public class RandomMutatorWindow
	{
		private enum State
		{
			Stopped,
			Started,
			Stopping
		}

		private readonly BehaviorSubject<State> _state = new BehaviorSubject<State>(State.Stopped);
		private readonly IObservable<Optional<AbsoluteFilePath>> _mainFile;

		//private Thread _thread;
		//private EventWaitHandle _stopSignal;

		public RandomMutatorWindow(IProject project)
		{
			var mutator = new RandomUxMutator();
			_mainFile =
				project.Documents
					.SelectPerElement(x => new { x.FilePath, x.Root })
					.WherePerElement(x => x.Root.Name.Is("App"))
					.Select(
						x =>
							x.Select(y => y.FilePath.Select(z => z.ToOptional()))
								.FirstOr(() => Observable.Return(Optional.None<AbsoluteFilePath>())))
					.Switch()
					.Replay(1)
					.RefCount();

			Observable.Interval(TimeSpan.FromSeconds(1))
				.CombineLatest(_mainFile.NotNone(), (_, f) => f)
				.ConnectWhile(_state.Is(State.Started))
				.Subscribe(
					x =>
					{
						try
						{
							mutator.Mutate(x, 10);
						}
						catch (Exception)
						{
							// ignored
						}
					});
		}

		void CreateWindow()
		{
			Application.Desktop.CreateSingletonWindow(
				Observable.Return(true),
				dialog => new Window
				{
					Title = Observable.Return("Auto ux mutator"),
					Size = Property.Create<Optional<Size<Points>>>(new Size<Points>(600, 260)).ToOptional(),
					Content = CreateContent()
				});
		}

		private IControl CreateContent()
		{
			return
				Layout.StackFromTop(
					Label.Create(
							_mainFile
								.Select(x => x.Select(path => "Start/stop random mutation of " + path.NativePath))
								.Or("No file with <App> tag found.")
								.AsText())
						.WithHeight(70)
						.Center(),
					Button.Create(clicked: _state.CombineLatest(_mainFile, CreateCommand).Switch())
						.Center(),
					Label.Create("DANGER: This will overwrite stuff in your file!")
						.WithHeight(70)
						.Center());
		}

		private Command CreateCommand(State state, Optional<AbsoluteFilePath> file)
		{
			switch (state)
			{
				case State.Stopping:
					return Command.Disabled;
				case State.Started:
					return Command.Enabled(Stop);
				case State.Stopped:
					return Command.Create(isEnabled: file.HasValue, action: Start);
				default:
					throw new InvalidOperationException("Unknown state " + state);
			}
		}

		private void Start()
		{
			_state.OnNext(State.Started);
		}

		private void Stop()
		{
			_state.OnNext(State.Stopped);
		}

		public static void Create(IProject project)
		{
			new RandomMutatorWindow(project).CreateWindow();
		}

		//async Task Run()
		//{
		//	var mutator = new RandomUxMutator();
		//}
	}
}