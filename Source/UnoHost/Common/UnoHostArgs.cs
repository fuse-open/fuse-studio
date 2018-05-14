using System;
using System.Collections.Generic;
using System.Linq;

namespace Outracks.UnoHost
{
	using IO;
	using IPC;

	public class UnoHostArgs
	{
		public static UnoHostArgs RemoveFrom(IList<string> args, IFileSystem fs)
		{
			var argsAsString = args.Select(a => "\"" + a + "\"").Join(" ");
			try
			{
				var unohostArgs = new UnoHostArgs()
				{
					AssemblyPath = args.TryRemoveAt(0)
						.SelectMany(fs.TryResolveAbsolutePath)
						.SelectMany(v => v is AbsoluteFilePath 
							? Optional.Some((AbsoluteFilePath)v) 
							: Optional.None())
						.OrThrow()
				};
				unohostArgs.IsDebug = args.TryGetAt(0).Select(arg => arg.Contains("--debug")).Or(false);
				if (unohostArgs.IsDebug) // We don't have the pipes so lets return
				{
					unohostArgs.UserDataPath = FilePath.CreateTempFile();
					return unohostArgs;
				}

				unohostArgs.InputPipe = args.TryRemoveAt(0).SelectMany(PipeName.TryParse).OrThrow();
				unohostArgs.OutputPipe = args.TryRemoveAt(0).SelectMany(PipeName.TryParse).OrThrow();
				unohostArgs.UserDataPath = args.TryRemoveAt(0).SelectMany(AbsoluteFilePath.TryParse).OrThrow();

				return unohostArgs;
			}
			catch (Exception)
			{
				throw new Exception("Failed to parse UnoHost arguments '" + argsAsString + "'");
			}
		}

		public IEnumerable<string> Serialize()
		{
			yield return AssemblyPath.NativePath;
			yield return InputPipe.ToString();
			yield return OutputPipe.ToString();
			yield return UserDataPath.ToString();
		}

		public AbsoluteFilePath MetadataPath
		{
			get { return AssemblyPath.ContainingDirectory / new FileName("metadata.json"); }
		}

		public AbsoluteFilePath AssemblyPath { get; set; }

		public PipeName InputPipe { get; set; }
		public PipeName OutputPipe { get; set; }
		public bool IsDebug { get; set; }
		public AbsoluteFilePath UserDataPath { get; set; }
	}
}