using System;
using System.Collections.Generic;
using System.Reactive;
using Outracks.Fusion;
using Outracks.Simulator;

namespace Outracks.Fuse
{
	/// <summary>
	/// This interface represents an _actual_ element instance, as opposed to an <see cref="IElement"/>
	/// which may also represent an actual element.
	/// </summary>
	public interface ILiveElement : IElement
	{
		IObservable<Unit> Changed { get; }

		new IBehaviorProperty<string> Name { get; }
		new IBehavior<ObjectIdentifier> SimulatorId { get; }
		new IBehavior<IReadOnlyList<ILiveElement>> Children { get; }
		new IBehaviorProperty<Optional<string>> this[string propertyName] { get; }
	}
}