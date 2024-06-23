using Uno.Build;
using Uno.Build.Targets;

namespace Fuse.Preview
{
	public static class PreviewTarget
	{
		public static readonly BuildTarget Android = new AndroidBuild();
		public static readonly BuildTarget iOS = new iOSBuild();
		public static readonly BuildTarget Native = new NativeBuild();
		public static readonly BuildTarget DotNet = new DotNetBuild();
	}
}
