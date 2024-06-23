using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Threading.Tasks;
using Outracks.IO;
using Outracks.Simulator;

namespace Outracks.Fuse
{
	public interface IProject : IDisposable
	{
		IContext Context { get; }

		IObservable<AbsoluteFilePath> FilePath { get; }

		IObservable<IImmutableSet<AbsoluteFilePath>> BundleFiles { get; }
		IObservable<IImmutableSet<AbsoluteFilePath>> FuseJsFiles { get; }

		IObservable<IImmutableList<IDocument>> Documents { get; }

		/// <summary>
		/// Creates a new document in the project, and adds it to unoproj file.
		/// Also creates any missing directories in path.
		/// </summary>
		Task CreateDocument(RelativeFilePath relativePath, SourceFragment contents = null);

		IElement GetElement(ObjectIdentifier simulatorId);

		IObservable<IEnumerable<IElement>> Classes { get; }

		IObservable<IEnumerable<IElement>> GlobalElements { get; }

		IObservable<AbsoluteDirectoryPath> RootDirectory { get; }
		IObservable<string> LogMessages { get; }
	}
}