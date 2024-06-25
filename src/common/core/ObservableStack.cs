using System;
using System.Collections.Immutable;
using System.Linq;
using System.Reactive.Linq;
using System.Reactive.Subjects;

namespace Outracks
{
	public class ObservableStack<T>
	{
		readonly BehaviorSubject<IImmutableStack<T>> _stack = new BehaviorSubject<IImmutableStack<T>>(ImmutableStack<T>.Empty);

		public ObservableStack()
		{
			Peek = _stack.Select(s => s.FirstOrNone());
			PeekUnder = _stack.Select(s => s.Skip(1).FirstOrNone());
		}

		public T Value
		{
			get { return _stack.Value.Peek(); }
		}

		public void Push(T value)
		{
			_stack.OnNext(_stack.Value.Push(value));
		}

		public T Pop()
		{
			var stack = _stack.Value;
			_stack.OnNext(stack.Pop());
			return stack.Peek();
		}

		public void Replace(params T[] stack)
		{
			_stack.OnNext(ImmutableStack.CreateRange(stack));
		}

		public IObservable<Optional<T>> Peek { get; private set; }
		public IObservable<Optional<T>> PeekUnder { get; private set; }
	}
}