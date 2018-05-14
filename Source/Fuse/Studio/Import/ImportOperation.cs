using System;
using Outracks.IO;
using System.IO;
using System.Collections.Generic;
using Outracks.Fuse.Designer;
using SketchConverter.API;
using Uno.Collections;

namespace Outracks.Fuse.Import
{
	public abstract class ImportOperation
	{
		public string Name { get; private set; }
		public string Description { get; private set; }

		protected ImportOperation(string name, string description)
		{
			Name = name;
			Description = description;
		}

		public abstract bool CanExecute(IAbsolutePath file, AbsoluteFilePath project);

		/// <exception cref="ImportFailed"></exception>
		public abstract void Execute(IAbsolutePath file, AbsoluteFilePath project, IEnumerable<string> arguments);

		public abstract void WriteOptions(TextWriter output);
	}

	public class SketchConversionOperation : ImportOperation
	{
		readonly IFileSystem _fileSystem;
		readonly IConverter _converter;
		readonly ReportLogger _reportLogger;

		public SketchConversionOperation(
			string name, 
			string description, 
			IFileSystem fileSystem, 
			IReport report) : base(name, description)
		{
			_fileSystem = fileSystem;
			_reportLogger = new ReportLogger(report);
			_converter = Factory.CreateConverterWithSymbolsUxBuilder(_reportLogger);
		}
		
		
		public override bool CanExecute(IAbsolutePath path, AbsoluteFilePath project)
		{
			var file = path as AbsoluteFilePath;
			if (file == null) return false;
			
			return _fileSystem.Exists(file) && _fileSystem.Exists(project) && 
				(SketchImportUtils.IsSketchFile(file) || file.Name.HasExtension(SketchImportUtils.SketchFilesExtension));
		}

		
		public override void Execute(IAbsolutePath path, AbsoluteFilePath project, IEnumerable<string> arguments)
		{
			var file = path as AbsoluteFilePath;
			
			if (file == null) throw new ImportFailed("'" + path + "' does not appear to be a file");
			
			var outputDir = project.ContainingDirectory.Combine(SketchImportUtils.OutputDirName);
			try
			{
				Directory.CreateDirectory(outputDir.NativePath);
			}
			catch (Exception e)
			{
				_reportLogger.Error("Sketch importer error: " + e.Message);
			}
						
			if (SketchImportUtils.IsSketchFile(file))
			{
				TryConvert(new[] { file.NativePath }, outputDir.NativePath);
			} 
			else if (SketchImportUtils.IsSketchFilesFile(file))
			{
				// load sketch file paths from the json-file
				var sketchFiles = SketchImportUtils.MakeAbsolute(	
					SketchImportUtils.SketchFilePaths(SketchImportUtils.SketchListFilePath(project), _reportLogger), 
					project.ContainingDirectory).Select(f => f.NativePath).ToArray();
				
				TryConvert(sketchFiles, outputDir.NativePath);
			}
		}


		public override void WriteOptions(TextWriter output)
		{
			output.WriteLine("Import Sketch Symbols some sketchy tips");
		}
		
		
		void TryConvert(IEnumerable<string> inputFiles, string outputDirectory)
		{
			try
			{
				_converter.Convert(inputFiles, outputDirectory);
			}
			catch (Exception e)
			{
				_reportLogger.Error("Sketch importer error: " + e.Message);
			}
		}
		
		class ReportLogger : ILogger
		{
			readonly IReport _report;

			public ReportLogger(IReport report) {_report = report;}
			public void Info(string info) { _report.Info(info, ReportTo.LogAndUser);}
			public void Warning(string warning) { _report.Warn(warning, ReportTo.LogAndUser);}
			public void Error(string error) { _report.Error(error);}
		}
	}
}