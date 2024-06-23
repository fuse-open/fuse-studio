using System;
using System.Threading.Tasks;

namespace Outracks.Fusion
{
	public static class FileDialog
	{
		public static Task<Optional<FileDialogResult>> OpenFile(FileDialogOptions options, Optional<IControl> windowControl = default(Optional<IControl>))
		{
			return Implementation(windowControl).OpenFile(options);
		}
		public static Func<Optional<IControl>, IFileDialogImplementation> Implementation;
	}
}