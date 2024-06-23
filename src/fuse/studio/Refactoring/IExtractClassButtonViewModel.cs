using System;
using Outracks.Fusion;

namespace Outracks.Fuse.Refactoring
{
	public interface IExtractClassButtonViewModel
	{
		Command Command { get; }
		IObservable<bool> HighlightSelectedElement { get; }
		void HoverEnter();
		void HoverExit();
		IObservable<string> Log { get; }
	}
}