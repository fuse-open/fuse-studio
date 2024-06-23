using System;

namespace Outracks.Building
{
	public class ExportTargetNotSupported : Exception
	{
		public readonly string ExportTarget;

		public ExportTargetNotSupported(string exportTarget)
			: base(exportTarget + " is not supported as a export target on this device.")
		{
			ExportTarget = exportTarget;
		}
	}
}