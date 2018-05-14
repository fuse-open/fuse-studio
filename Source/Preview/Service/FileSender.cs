using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using System.Reactive.Linq;
using Outracks;
using Outracks.IO;
using Outracks.Simulator.Bytecode;
using Outracks.Simulator.CodeGeneration;
using Outracks.Simulator.Protocol;

namespace Fuse.Preview
{
	public class FileSender<T>
	{
		readonly Func<FileDataWithMetadata<T>, IObservable<Statement>> _statementToExecute;

		public FileSender(Func<FileDataWithMetadata<T>, IObservable<Statement>> statementToExecute)
		{
			_statementToExecute = statementToExecute;
		}

		public IObservable<CoalesceEntry> CreateMessages(FileDataWithMetadata<T> file)
		{
			try
			{
				return _statementToExecute(file)
					.Select(s => new[] { s }.MakeExecute())
					.ToCoalesceEntry(file.Metadata.ToString(), addFirst: true);
			}
			catch (Exception)
			{
				return Observable.Return(new BytecodeUpdated(new Lambda(Signature.Action(), Enumerable.Empty<BindVariable>(), Enumerable.Empty<Statement>())))
					.ToCoalesceEntry("invalid-file-dependency");
			}
		}
	}

	public static class MakeExecuteExtension
	{
		public static BytecodeUpdated MakeExecute(this IEnumerable<Statement> statements)
		{
			return new BytecodeUpdated(MakeLambda(statements));
		}

		public static Lambda MakeLambda(this IEnumerable<Statement> statements)
		{
			return new Lambda(
				new Signature(Outracks.Simulator.ImmutableList<Parameter>.Empty, Optional.None()),
				localVariables: new BindVariable[0],
				statements: statements);
		}
	}

	public static class FileSourceSender
	{
		public static FileSender<ProjectDependency> Create(IFileSystem fileSystem)
		{
			return new FileSender<ProjectDependency>(
				statementToExecute: file => 
					Observable.Return(ImportExpression.UpdateFile(
						descriptor: file.Metadata.Descriptor, 
						data: file.Data)));
		}
	}

	public static class BundleFileSender
	{
		public static FileSender<AbsoluteFilePath> Create(IFileSystem fileSystem, IObservable<AbsoluteDirectoryPath> project)
		{
			return new FileSender<AbsoluteFilePath>(
				statementToExecute: file => 
					project.Select(projDir => 
						BundleFiles.AddOrUpdateFile(
							descriptor: file.Metadata.RelativeTo(projDir).NativeRelativePath.ToUnixPath(),
							data: file.Data)));
		}
	}
}