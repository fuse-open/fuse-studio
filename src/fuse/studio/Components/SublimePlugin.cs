using System;
using Outracks.IO;

namespace Outracks.Fuse.Components
{
	class SublimePlugin : ScriptInstaller
	{
		public SublimePlugin(AbsoluteDirectoryPath componentsDir)
			: base("sublime-plugin", "Install Fuse plugin for Sublime Text 3", componentsDir)
		{
		}
	}
}
