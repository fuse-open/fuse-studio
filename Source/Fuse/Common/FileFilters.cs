
namespace Outracks
{
	public static class FileFilters
	{
		public static FileFilter UnoFiles
		{
			get { return new FileFilter("Uno source file", "uno"); }
		}

		public static FileFilter UxFiles
		{
			get { return new FileFilter("UX file", "ux"); }
		}

		public static FileFilter UxlFiles
		{
			get { return new FileFilter("UXL file", "uxl"); }
		}

		public static FileFilter SourceFiles
		{
			get { return FileFilter.Union("Source Files", UnoFiles, UxFiles, UxlFiles); }
		}

		public static FileFilter UnoProjects
		{
			get { return new FileFilter("Uno Projects", "unoproj", "unosln"); }
		}

		public static FileFilter AllFiles
		{
			get { return new FileFilter("All Files"); }
		}
	}
}