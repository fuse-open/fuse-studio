using System;
using Outracks.Diagnostics;
using Outracks.IO;

namespace Outracks.Fuse
{
	
	public interface IFuse : IFuseLauncher
	{
		OS Platform { get; }
		string Version { get; }
		bool IsInstalled { get; }

		Guid SystemId { get; }
		Guid SessionId { get; }

		AbsoluteDirectoryPath FuseRoot { get; }
		AbsoluteDirectoryPath ModulesDir { get; }

		AbsoluteFilePath FuseExe { get; }
		AbsoluteFilePath UnoExe { get; }
		Optional<AbsoluteFilePath> MonoExe { get; }

		IExternalApplication Uno { get; }
		IExternalApplication Tray { get; }
		IExternalApplication Designer { get; }

		IExternalApplication CodeAssistance { get; }
		IExternalApplication LogServer { get; }

		AbsoluteDirectoryPath UserDataDir { get; }
		AbsoluteDirectoryPath ProjectsDir { get; }
		IReport Report { get; }
	}
}
