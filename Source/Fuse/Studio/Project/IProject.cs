using System;
using System.Collections.Generic;
using System.Collections.Immutable;

namespace Outracks.Fuse
{
	using IO;
	using Simulator;

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
		System.Threading.Tasks.Task CreateDocument(RelativeFilePath relativePath, SourceFragment contents = null);

		IElement GetElement(ObjectIdentifier simulatorId);

		IObservable<IEnumerable<IElement>> Classes { get; } 
		
		IObservable<IEnumerable<IElement>> GlobalElements { get; }

		IObservable<AbsoluteDirectoryPath> RootDirectory { get; }
		IObservable<string> LogMessages { get; }
	}
}