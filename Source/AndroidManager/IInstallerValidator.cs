using System;
using Outracks.IO;

namespace Outracks.AndroidManager
{
	public interface IInstallerValidator
	{
		bool IsInstalledAt(AbsoluteDirectoryPath installPath, IProgress<InstallerEvent> progress);
	}
}