using System;
using Outracks.IO;

namespace Outracks.Fuse.Components
{
	[Obsolete]
	class VsCodePlugin : ScriptInstaller
	{
		public VsCodePlugin(AbsoluteDirectoryPath componentsDir)
			: base("vscode-plugin", "Install Fuse extension for Visual Studio Code (legacy)", componentsDir, "vscode-extension")
		{
		}
	}
}
