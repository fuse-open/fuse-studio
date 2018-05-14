using System;
using System.Collections.Generic;
using System.Reactive.Linq;
using System.Threading.Tasks;
using System.Xml.Linq;

namespace Outracks.Fuse
{
	using Fusion;
	using Simulator;

	public static partial class Element
	{
		static Element()
		{
			Empty = new EmptyElement();
		}

		public static IElement Empty { get; private set; }
	}

	class EmptyElement : IElement
	{
		readonly IProperty<Optional<string>> _property;
		readonly IObservable<bool> _false;

		public EmptyElement()
		{
			_false = Observable.Return(false);
			_property = Property.Default<Optional<string>>();
			Children = Observable.Return(System.Collections.Immutable.ImmutableList<IElement>.Empty);
			Name = Property.Constant("Unknown");
			SimulatorId = Observable.Return(new ObjectIdentifier("N/A", 0));
		}

		public IProperty<string> Name { get; private set; }

		public IObservable<IEnumerable<IElement>> Children { get; private set; }

		/// <exception cref="ElementIsEmpty"></exception>
		/// <exception cref="ElementIsReadOnly"></exception>
		public Task Replace(Func<SourceFragment, System.Threading.Tasks.Task<SourceFragment>> transform)
		{
			throw new ElementIsEmpty();
		}

		public System.Threading.Tasks.Task<SourceFragment> Cut()
		{
			throw new ElementIsEmpty();
		}

		public System.Threading.Tasks.Task<SourceFragment> Copy()
		{
			throw new ElementIsEmpty();
		}

		public IElement Paste(SourceFragment fragment)
		{
			throw new ElementIsReadOnly();
		}

		public IElement PasteAfter(SourceFragment fragment)
		{
			throw new ElementIsReadOnly();
		}

		public IElement PasteBefore(SourceFragment fragment)
		{
			throw new ElementIsReadOnly();
		}

		public IProperty<Optional<string>> Content
		{
			get { return _property; }
		}

		public IObservable<Optional<ILiveElement>> LiveElement
		{
			get { return Observable.Return(Optional.None<ILiveElement>()); }
		}

		public IObservable<bool> IsEmpty
		{
			get { return Observable.Return(true); }
		}

		public IObservable<bool> IsReadOnly
		{
			get { return Observable.Return(true); }
		}

		public IElement Parent
		{
			get { return Element.Empty; }
		}

		public IElement Base
		{
			get { return Element.Empty; }
		}

		public IObservable<ObjectIdentifier> SimulatorId { get; private set; }
		
		public IObservable<bool> Is(string elementType)
		{
			return _false;
		}

		public IObservable<bool> IsChildOf(string elementType)
		{
			return _false;
		}

		public IObservable<bool> IsSiblingOf(string elementType)
		{
			return _false;
		}

		public IObservable<bool> IsDescendantOf(IElement element)
		{
			return _false;
		}

		public IProperty<Optional<string>> this[string propertyName]
		{
			get { return _property; }
		}

		public IObservable<Optional<SourceReference>> SourceReference
		{
			get { return Observable.Return(new Optional<SourceReference>()); }
		}
	}
}