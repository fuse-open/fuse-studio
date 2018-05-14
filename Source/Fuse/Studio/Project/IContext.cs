using System;
using System.Threading.Tasks;

namespace Outracks.Fuse
{
	public interface IContext
	{
		// Selection

		IElement CurrentSelection { get; }

		IElement PreviewedSelection { get; }

		Task Select(IElement element);

		Task Preview(IElement element);

		IObservable<bool> IsSelected(IElement element);
		IObservable<bool> IsPreviewSelected(IElement element);

		// Scope

		IElement CurrentScope { get; }

		IElement PreviousScope { get; }

		Task SetScope(IElement root, IElement selection);

		Task PushScope(IElement root, IElement selection);

		Task PopScope();
	}
}