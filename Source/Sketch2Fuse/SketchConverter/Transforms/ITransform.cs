using SketchImporter.UxGenerator;

namespace SketchConverter.Transforms
{
	public interface ITransform
	{
		void Apply(UxNode uxClass);
	}
}
