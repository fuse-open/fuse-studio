using System;
using Outracks.IO;

namespace Outracks.Fuse.Components
{
	class VsCodeExtension : ScriptInstaller
	{
		public VsCodeExtension(AbsoluteDirectoryPath componentsDir)
			: base("vscode-extension", "Install Fuse extension for Visual Studio Code", componentsDir)
		{
		}
	}
}
