using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Threading.Tasks;

namespace Outracks.Fuse
{
	using Fusion;

	/// <summary>
	/// An interface for observing and potentially editing a UX element/tag. 
	/// 
	/// You should consider instances as observable query results, rather than (wrongly) assuming 
	/// there a one-to-one mapping between IElement instances and the data it's an interface to.
	/// 
	/// (If you really care about element identity, observe SimulatorId. Don't rely on equality operators of IElement)
	/// </summary>
	public interface IElement
	{
		IObservable<Optional<ILiveElement>> LiveElement { get; }

		IObservable<bool> IsEmpty { get; }
		IObservable<bool> IsReadOnly { get;  } 
		
		IElement Parent { get; }
		IElement Base { get; }
		
		IObservable<Simulator.ObjectIdentifier> SimulatorId { get; }
		IObservable<Optional<Simulator.SourceReference>> SourceReference { get; }

		IProperty<string> Name { get; }
		IObservable<bool> Is(string elementType);
		IObservable<bool> IsChildOf(string elementType);
		IObservable<bool> IsSiblingOf(string elementType);
		IObservable<bool> IsDescendantOf(IElement element);

		IProperty<Optional<string>> this[string propertyName] { get; }
		IProperty<Optional<string>> Content { get; }

		IObservable<IEnumerable<IElement>> Children { get; }

		/// <exception cref="ElementIsEmpty"></exception>
		/// <exception cref="ElementIsReadOnly"></exception>
		Task Replace(Func<SourceFragment, Task<SourceFragment>> transform);

		/// <exception cref="ElementIsEmpty"></exception>
		/// <exception cref="ElementIsReadOnly"></exception>
		/// <returns>The source fragment that was removed from the document</returns>
		Task<SourceFragment> Cut();

		/// <exception cref="ElementIsEmpty"></exception>
		/// <returns>A snapshot of the source fragment for this element</returns>
		Task<SourceFragment> Copy();

		/// <exception cref="ElementIsReadOnly"></exception>
		/// <returns>A snapshot of the source fragment for this element</returns>
		IElement Paste(SourceFragment fragment);
	
		/// <exception cref="ElementIsReadOnly"></exception>
		/// <exception cref="ElementIsRoot"></exception>
		/// <returns>A snapshot of the source fragment for this element</returns>
		IElement PasteAfter(SourceFragment fragment);

		/// <exception cref="ElementIsReadOnly"></exception>
		/// <exception cref="ElementIsRoot"></exception>
		/// <returns>An interface to the pasted element</returns>
		IElement PasteBefore(SourceFragment fragment);
	}

	public class ElementIsEmpty : InvalidOperationException
	{
		public override string Message
		{
			get { return "Element is empty"; }
		}
	}

	public class ElementIsReadOnly : InvalidOperationException
	{
		public override string Message
		{
			get { return "Element is read-only"; }
		}
	}

	public class ElementIsRoot : InvalidOperationException
	{
		public override string Message
		{
			get { return "Element is root"; }
		}
	}
	
}