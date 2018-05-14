using System.IO;
using System.Linq;
using SketchConverter.API;
using SketchConverter.SketchModel;
using SketchImporter.UxGenerator;

namespace SketchConverter.UxBuilder
{
	public class ArtboardUxBuilder : IUxBuilder
	{
		private readonly ILogger _log;

		public ArtboardUxBuilder(ILogger log)
		{
			_log = log;
		}

		public void Build(SketchDocument document, string outputDirectory)
		{
			var symbols = document
				.Pages
				.AsEnumerable()
				.SelectMany(page => page.Layers.OfType<SketchSymbolMaster>());

			var symbolClassNameBuilder = new SymbolClassNameBuilder();
			symbolClassNameBuilder.Init(symbols);

			var appNode = new UxNode { ClassName = "App" };
			var pageControl = new UxNode { ClassName = "PageControl" };
			appNode.Children.Add(pageControl);

			var artboards = document.Pages
				.AsEnumerable()
				.Reverse()
				.SelectMany(page => page.Layers
					.OfType<SketchArtboard>()
					.Reverse());

			var builder = new UxBuilder(symbolClassNameBuilder, new AssetEmitter(outputDirectory, _log), _log);

			var pages = artboards.Select(builder.BuildPage);
			pageControl.Children.AddRange(pages);

			var serializerContext = new UxSerializerContext();
			var ux = appNode.SerializeUx(serializerContext);

			var outputFilePath =  Path.Combine(outputDirectory, "MainView.ux");
			File.WriteAllText(outputFilePath, ux);
		}
	}
}
