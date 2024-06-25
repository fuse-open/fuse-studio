using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using SketchConverter.API;
using SketchConverter.SketchModel;
using SketchConverter.Transforms;
using SketchImporter.UxGenerator;

namespace SketchConverter.UxBuilder
{
	public class SymbolsUxBuilder : IUxBuilder
	{
		private readonly ILogger _log;

		public SymbolsUxBuilder(ILogger log)
		{
			_log = log;
		}

		public void Build(SketchDocument document, string outputDirectory)
		{
			var symbols = document
				.Pages
				.AsEnumerable()
				.SelectMany(page => page.Layers.OfType<SketchSymbolMaster>())
				.Where(s => NameIsValidOrLog(s.Name))
				.ToList();

			//We don't actually need to remove duplicates, since the ux builder will overwrite symbols with the same name anyway.
			LogDuplicateSymbolNames(symbols);

			if (!symbols.Any())
			{
				_log.Info("No UX generated because no Sketch symbols found in sketch file. Sketchy :)");
				return;
			}

			var symbolClassNameBuilder = new SymbolClassNameBuilder();
			symbolClassNameBuilder.Init(symbols);

			var builder = new UxBuilder(symbolClassNameBuilder, new AssetEmitter(outputDirectory, _log),  _log);
			var serializerContext = new UxSerializerContext();

			// write one file per uxClass
			foreach (var symbol in symbols)
			{
				try
				{
					var uxClass = builder.BuildSymbolClass(symbol);
					ApplyTransforms(uxClass);
					var ux = uxClass.SerializeUx(serializerContext);
					var className = uxClass.Attributes["ux:Class"] as UxString;
					if (className != null)
					{
						if (symbol.Layers.Any())
						{
							var outputFilePath = Path.Combine(outputDirectory, className.Value + ".ux");
							try
							{
								File.WriteAllText(outputFilePath, ux);
								_log.Info($"Wrote '{className.Value}' to '{outputFilePath}'");
							}
							catch (Exception e)
							{
								_log.Error("Can't write file '" + outputFilePath + "' " + e.Message);
							}
						}
						else
						{
							_log.Warning($"Skipping symbol '{className.Value}' which has no supported layers.");
						}
					}
					else
					{
						_log.Error("Can't write file for ux:Class without name");
					}
				}
				catch (Exception e)
				{
					_log.Error("Failed to convert '" + symbol.Name + "': " + e.Message);
				}
			}
		}

		private void ApplyTransforms(UxNode uxClass)
		{
			var transforms = new ITransform[] {new TextPropertyTransform(_log)};
			foreach (var transform in transforms)
			{
				transform.Apply(uxClass);
			}
		}

		private bool NameIsValidOrLog(string name)
		{
			var validity = NameValidator.NameIsValid(name);
			if (validity == NameValidity.InvalidCharacter)
			{
				_log.Error($"The symbol name '{name}' contains an invalid character. Please only use the letters a-z, numbers, or underscores, and don't start the name with a number.");
				return false;
			}
			if (validity == NameValidity.InvalidKeyword)
			{
				_log.Error($"The symbol name '{name}' is a reserved word. Please choose another name.");
				return false;
			}
			return true;
		}

		private void LogDuplicateSymbolNames(IEnumerable<SketchSymbolMaster> symbols)
		{
			var seen = new HashSet<string>();
			foreach (var symbol in symbols)
			{
				if (seen.Contains(symbol.Name))
				{
					_log.Error($"More than one symbol named '{symbol.Name}' was found! Only generating ux for one of them. Please make sure symbol names are unique.");
				}
				seen.Add(symbol.Name);
			}
		}
	}
}
