using System;
using Outracks.IO;

namespace Outracks.Fuse.Components
{
	class AndroidBuildTools : ScriptInstaller
	{
		public AndroidBuildTools(AbsoluteDirectoryPath componentsDir)
			: base("android", "Install all required components to build for Android", componentsDir)
		{
		}
	}
}
