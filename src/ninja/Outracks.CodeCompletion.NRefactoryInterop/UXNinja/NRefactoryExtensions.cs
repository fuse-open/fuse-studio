using ICSharpCode.NRefactory.Editor;

namespace Outracks.CodeCompletionFactory.UXNinja
{
	static class NRefactoryExtensions
	{
		public static bool IsOffsetInsideSegment(this ISegment segment, int offset)
		{
			return segment.Offset < offset && offset < segment.EndOffset;
		}
	}
}