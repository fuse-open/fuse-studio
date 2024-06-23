using SketchConverter.SketchModel;

namespace SketchConverter.UxBuilder
{
	public interface IUxBuilder
	{
		void Build(SketchDocument document, string outputDirectory);
	}
}
