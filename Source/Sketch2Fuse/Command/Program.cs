using System;
using System.Collections.Generic;
using Mono.Options;
using SketchConverter;
using SketchConverter.SketchParser;
using SketchConverter.UxBuilder;

namespace Command
{
	internal static class Program
	{
		public static void Main(string[] args)
		{
			var sketchFilePaths = new List<string>();
			string outputDir = null;
			#pragma warning disable 0219
			var watch = false;
			#pragma warning restore 0219
			var showHelp = false;
			var app = false;

			var options = new OptionSet
			{
				{"i=", "Sketch file to import. Specify multiple sketch-files by using the -i option for each file.", p => sketchFilePaths.Add(p)},
				{"o=", "Destination Fuse project directory", p => outputDir = p},
				{"a|app", "Write output to MainView.ux inside an app tag", a => app = (a != null)},
				{"w|watch", "Watch .sketch file for changes", w => watch = (w != null)},
				{"h|?|help", "Display this help message", v => showHelp = (v != null)}
			};
			try {
				var extra = options.Parse(args);
				if (extra.Count > 0)
				{
					foreach (var arg in extra)
					{
						Console.WriteLine("Unknown argument: '" + arg + "'");
						PrintUsage(options);
					}
					Environment.Exit(1);
				}
			}
			catch(OptionException e)
			{
				Console.Error.WriteLine("Error: " + e.Message);
				PrintUsage(options);
				Environment.Exit(1);
			}

			if (showHelp)
			{
				PrintUsage(options);
				Environment.Exit(0);
			}

			if (sketchFilePaths.Count == 0 || string.IsNullOrEmpty(outputDir))
			{
				PrintUsage(options);
				Environment.Exit(1);
			}

			try
			{
				var logger = new Logger();
				var builder = app ? (IUxBuilder) new ArtboardUxBuilder(logger) : (IUxBuilder) new SymbolsUxBuilder(logger);
				var parser = new SketchParser(logger);
				var converter = new Converter(parser, builder, logger);
				converter.Convert(sketchFilePaths, outputDir);
			}
			catch (Exception e)
			{
				Console.Error.WriteLine("Error: " + e.Message);
				Environment.Exit(1);
			}

		}

		static void PrintUsage(OptionSet opts)
		{
			Console.WriteLine("Usage: SketchImporter [--watch] -i SketchFile.sketch -o MyFuseProject/");
			Console.WriteLine();
			Console.WriteLine("Options:");
			opts.WriteOptionDescriptions(Console.Out);
		}

	}
}
