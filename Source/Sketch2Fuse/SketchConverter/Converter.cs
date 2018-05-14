using System;
using System.Collections.Generic;
using System.IO;
using Mono.Unix;
using Mono.Unix.Native;
using SketchConverter.API;
using SketchConverter.SketchParser;
using SketchConverter.UxBuilder;

namespace SketchConverter
{
	public class Converter : IConverter
	{
		private readonly IUxBuilder _builder;
		private readonly ISketchParser _parser;
		private readonly ILogger _log;

		public Converter(ISketchParser parser, IUxBuilder builder, ILogger log)
		{
			_parser = parser;
			_builder = builder;
			_log = log;
		}

		public void Convert(IEnumerable<string> sketchFilePath, string outputdirPath)
		{
			if (OutputDirectoryNotValid(outputdirPath)) return;
			var validInputPaths = ValidInputFiles(sketchFilePath);

			foreach (var validInputPath in validInputPaths)
			{
				using (var stream = File.Open(validInputPath, FileMode.Open))
				using (var sketchArchive = new SketchArchive())
				{
					try
					{
						sketchArchive.Load(stream);
						CompatibilityChecker.Check(validInputPath, sketchArchive, _log);
						var document = _parser.Parse(sketchArchive);
						_builder.Build(document, outputdirPath);
					}
					catch (AggregateException ae)
					{
						foreach (var e in ae.Flatten().InnerExceptions)
						{
							_log.Error(validInputPath + " : " + e.Message);
						}
					}
					catch (Exception e)
					{
						_log.Error(validInputPath + " : " + e.Message);
					}
				}
			}

		}

		private IEnumerable<string> ValidInputFiles(IEnumerable<string> inputFilePaths)
		{

			foreach (var sketchFilePath in inputFilePaths)
			{

				if (!File.Exists(sketchFilePath))
				{
					_log.Error("Can't convert non-existing Sketch-file " + sketchFilePath);
				}
				else
				{
					yield return sketchFilePath;
				}
			}
		}

		private bool OutputDirectoryNotValid(string outputdirPath)
		{

			if (!Directory.Exists(outputdirPath))
			{
				_log.Error("Output directory " + outputdirPath + " does not exist");
				return true;
			}

			if (Environment.OSVersion.Platform == PlatformID.MacOSX ||
				Environment.OSVersion.Platform == PlatformID.Unix)
			{
				var ufi = new UnixFileInfo(outputdirPath);
				// accessible for writing
				if (!ufi.CanAccess(AccessModes.W_OK))
				{
					_log.Error("Can't access output directory " + outputdirPath);
					return true;
				}
			}
			else
			{
				try
				{
					Directory.GetAccessControl(outputdirPath);
				}
				catch (UnauthorizedAccessException e)
				{
					_log.Error(e.Message);
					return true;
				}
			}
			return false;
		}
	}
}
