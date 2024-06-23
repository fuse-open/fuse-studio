using System;
using System.Collections.Generic;
using System.Reactive.Linq;
using System.Threading.Tasks;
using Outracks.Simulator;

namespace Outracks.Fuse
{
	public static partial class Element
	{
		public static IElement ToElement<T>(this IObservable<T> source, Func<T, IElement> select)
		{
			return new SwitchingElement(source.Select(@select));
		}

		/// <remarks>
		/// Note that queries on the result requring actual data might not push a value until the observable has pushed the first element.
		/// If it is desirable to to start with a specific element (for instance Element.Empty), use StartWith(intialElement) before Switch().
		/// </remarks>>
		public static IElement Switch(this IObservable<IElement> element)
		{
			return new SwitchingElement(element);
		}
	}

	class SwitchingElement : IElement
	{
		readonly IObservable<IElement> _current;

		readonly Lazy<IElement> _parent;
		public IElement Parent
		{
			get { return _parent.Value; }
		}

		readonly Lazy<IElement> _base;
		public IElement Base
		{
			get { return _base.Value; }
		}

		public SwitchingElement(IObservable<IElement> current)
		{
			_current = current
				.DistinctUntilChanged()
				.Replay(1).RefCount()
				;

			_parent = new Lazy<IElement>(() =>
				_current.Select(c => c.Parent).Switch());

			_base = new Lazy<IElement>(() =>
				_current.Select(c => c.Base).Switch());

			Name = _current.Select(e => e.Name).Switch()
				;

			Content = _current.Select(e => e.Content).Switch()
				;

			Children = _current.Select(e => e.Children).Switch()
				.DistinctUntilSequenceChanged()
				.Replay(1).RefCount()
				;

			IsEmpty = _current.Select(e => e.IsEmpty).Switch()
				.DistinctUntilChanged()
				.Replay(1).RefCount()
				;

			IsReadOnly = _current.Select(e => e.IsReadOnly).Switch()
				.DistinctUntilChanged()
				.Replay(1).RefCount()
				;

			SimulatorId = _current.Select(e => e.SimulatorId).Switch()
				.DistinctUntilChanged()
				.Replay(1).RefCount()
				;

			SourceReference = _current.Select(e => e.SourceReference).Switch()
				.DistinctUntilChanged()
				.Replay(1).RefCount()
				;
		}

		public IProperty<string> Name { get; private set; }

		public IProperty<Optional<string>> Content { get; private set; }

		public IObservable<IEnumerable<IElement>> Children { get; private set; }

		public IProperty<Optional<string>> this[string propertyName]
		{
			get { return _current.Select(c => c[propertyName]).Switch(); }
		}

		public IObservable<Optional<ILiveElement>> LiveElement { get { return _current.Switch(x => x.LiveElement); } }
		public IObservable<bool> IsEmpty { get; private set; }

		public IObservable<bool> IsReadOnly { get; private set; }

		public IObservable<ObjectIdentifier> SimulatorId { get; private set; }

		public IObservable<Optional<SourceReference>> SourceReference { get; private set; }

		public async Task Replace(Func<SourceFragment, System.Threading.Tasks.Task<SourceFragment>> transform)
		{
			await (await _current.FirstAsync()).Replace(transform);
		}

		public async System.Threading.Tasks.Task<SourceFragment> Cut()
		{
			return await (await _current.FirstAsync()).Cut();
		}

		public async System.Threading.Tasks.Task<SourceFragment> Copy()
		{
			return await (await _current.FirstAsync()).Copy();
		}

		public IElement Paste(SourceFragment fragment)
		{
			var result = _current.FirstAsync().Select(c => c.Paste(fragment)).Replay(1);
			result.Connect();
			return result.Switch();
		}

		public IElement PasteAfter(SourceFragment fragment)
		{
			var result = _current.FirstAsync().Select(c => c.PasteAfter(fragment)).Replay(1);
			result.Connect();
			return result.Switch();
		}

		public IElement PasteBefore(SourceFragment fragment)
		{
			var result = _current.FirstAsync().Select(c => c.PasteBefore(fragment)).Replay(1);
			result.Connect();
			return result.Switch();
		}


		public IObservable<bool> Is(string elementType)
		{
			return _current.Switch(c => c.Is(elementType)).DistinctUntilChanged();
		}

		public IObservable<bool> IsDescendantOf(IElement element)
		{
			return _current.Select(c => c.IsDescendantOf(element)).Switch();
		}

		public IObservable<bool> IsChildOf(string elementType)
		{
			return _current.Switch(c => c.IsChildOf(elementType)).DistinctUntilChanged();
		}

		public IObservable<bool> IsSiblingOf(string elementType)
		{
			return _current.Switch(c => c.IsSiblingOf(elementType)).DistinctUntilChanged();
		}
	}
}