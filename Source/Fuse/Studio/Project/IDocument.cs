using System;
using System.Collections.Generic;

namespace Outracks.Fuse
{
	using IO;

	public interface IDocument : IDisposable
	{
		IObservable<Optional<Exception>> Errors { get; }

		IObservable<AbsoluteFilePath> FilePath { get; } 

		string SimulatorIdPrefix { get; }

		IElement Root { get; }

		IObservable<IEnumerable<IElement>> Elements { get; }
	}
}