using System;
using Outracks.Fuse.Auth;
using Outracks.IO;

namespace Outracks.Fuse
{
	public interface IFuse : IFuseLauncher, ILicenseState
	{
		string Version { get; }
		string CommitSha { get; }

		Guid SystemId { get; }
		Guid SessionId { get; }

		AbsoluteDirectoryPath FuseRoot { get; }
		AbsoluteDirectoryPath ComponentsDir { get; }

		AbsoluteFilePath FuseExe { get; }

		IExternalApplication Uno { get; }
		IExternalApplication Tray { get; }
		IExternalApplication Studio { get; }

		IExternalApplication CodeAssistance { get; }
		IExternalApplication LogServer { get; }
		IExternalApplication UnoHost { get; }

		AbsoluteDirectoryPath UserDataDir { get; }
		AbsoluteDirectoryPath ProjectsDir { get; }
		IReport Report { get; }
		ILicense License { get; }
	}
}
