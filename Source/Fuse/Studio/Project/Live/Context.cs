using System;
using System.Reactive;
using System.Reactive.Linq;
using System.Reactive.Subjects;
using System.Threading.Tasks;
using ObjectIdentifier = Outracks.Simulator.ObjectIdentifier;

namespace Outracks.Fuse.Live
{
	class ContextState
	{
		public ObjectIdentifier Root;
		public ObjectIdentifier CurrentSelection;
	}

	class Context : IContext
	{
		class Scope
		{
			public IElement Root;
			public BehaviorSubject<IElement> PreviewedSelection;
			public BehaviorSubject<IElement> CurrentSelection;
		}

		readonly ObservableStack<Scope> _scope = new ObservableStack<Scope>();
		readonly ISubject<ContextState> _state = new Subject<ContextState>();
		readonly Scope _root;
		readonly Func<ObjectIdentifier, IElement> _getElement; 

		public Context(IElement app, Func<ObjectIdentifier, IElement> getElement)
		{
			_getElement = getElement;

			CurrentSelection = _scope.Peek
				.Select(scope => scope.MatchWith(
					some: s => s.CurrentSelection, 
					none: () => Observable.Return(Element.Empty)))
				.Switch()
				.Switch();

			PreviewedSelection = _scope.Peek
				.Select(scope => scope.MatchWith(
					some: s => s.PreviewedSelection, 
					none: () => Observable.Return(Element.Empty)))
				.Switch()
				.Switch();

			CurrentScope = _scope.Peek
				.SelectPerElement(scope => scope.Root)
				.Or(Element.Empty).Switch();

			PreviousScope = _scope.PeekUnder
				.SelectPerElement(scope => scope.Root)
				.Or(Element.Empty).Switch();

			_scope.Replace(_root = new Scope
			{
				Root = app,
				CurrentSelection = new BehaviorSubject<IElement>(Element.Empty),
				PreviewedSelection = new BehaviorSubject<IElement>(Element.Empty),
			});
		}

		public IObservable<ContextState> State
		{
			get { return _state; }
		}

		// Selection

		public IElement CurrentSelection { get; private set; }

		public IElement PreviewedSelection { get; private set; }

		public async Task Select(IElement element)
		{
			var state = await RecordState(_scope.Value.Root, element);
			_scope.Value.CurrentSelection.OnNext(_getElement(state.CurrentSelection));
		}

		public Task Preview(IElement element)
		{
			_scope.Value.PreviewedSelection.OnNext(element);
			return Task.FromResult(Unit.Default);
		}

		public IObservable<bool> IsSelected(IElement element)
		{
			return CurrentSelection.IsSameAs(element);
		}

		public IObservable<bool> IsPreviewSelected(IElement element)
		{
			return PreviewedSelection.IsSameAs(element);
		}

		// Scope

		public IElement CurrentScope { get; private set; }

		public IElement PreviousScope { get; private set; }

		public async Task PopScope()
		{
			_scope.Pop();
			var scope = _scope.Value;
			await RecordState(scope.Root, scope.CurrentSelection.Value);
		}

		public async Task PushScope(IElement root, IElement selection)
		{
			var state = await RecordState(root, selection);

			_scope.Push(new Scope
			{
				Root = _getElement(state.Root),
				CurrentSelection = new BehaviorSubject<IElement>(_getElement(state.CurrentSelection)),
				PreviewedSelection = new BehaviorSubject<IElement>(Element.Empty),
			});
		}

		public async Task SetScope(IElement root, IElement selection)
		{
			var state = await RecordState(root, selection);

			_scope.Replace(_root, new Scope
			{
				Root = _getElement(state.Root),
				CurrentSelection = new BehaviorSubject<IElement>(_getElement(state.CurrentSelection)),
				PreviewedSelection = new BehaviorSubject<IElement>(Element.Empty),
			});
		}

		async Task<ContextState> RecordState(IElement root, IElement selection)
		{
			var state = new ContextState
			{
				Root = await root.SimulatorId.FirstAsync(),
				CurrentSelection = await selection.SimulatorId.FirstAsync(),
			};

			_state.OnNext(state);

			return state;
		}
	}
}