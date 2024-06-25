using System;

namespace Outracks.IO
{
	[Flags]
	public enum FileSystemPermission
	{
		Read = 1 << 0,
		Write = 1 << 1,
		Execute = 1 << 2
	}

	[Flags]
	public enum FileSystemGroup
	{
		Group = 1 << 0,
		User = 1 << 1,
		Others = 1 << 2,
		Everyone = Group | User | Others
	}

}
