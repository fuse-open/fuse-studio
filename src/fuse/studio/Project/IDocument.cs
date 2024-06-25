using System;
using System.Collections.Generic;
using Outracks.IO;

namespace Outracks.Fuse
{
	public interface IDocument : IDisposable
	{
		IObservable<Optional<Exception>> Errors { get; }

		IObservable<AbsoluteFilePath> FilePath { get; }

		string SimulatorIdPrefix { get; }

		IElement Root { get; }

		IObservable<IEnumerable<IElement>> Elements { get; }
	}
}