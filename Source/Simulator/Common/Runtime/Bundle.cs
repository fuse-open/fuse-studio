using Uno.Collections;

namespace Outracks.Simulator.Runtime
{
	public static class Bundle
	{
		static Uno.IO.Bundle _bundle;
		public static void Initialize(string projectName)
		{
			_bundle = Uno.IO.Bundle.Get(projectName);
		}

		extern(PREVIEW)
		public static void AddOrUpdateFile(string projectRelativeFilePath, byte[] data)
		{
			foreach (var exisitingFile in _bundle.Files)
			{
				if (exisitingFile.SourcePath == projectRelativeFilePath)
				{
					//debug_log "Update " + projectRelativeFilePath + " with " + data.Length + " bytes";
					exisitingFile.Update(data);
					return;
				}
			}

			//debug_log "Create " + projectRelativeFilePath + " with " + data.Length + " bytes";
			_bundle.CreateFile(projectRelativeFilePath, data);
		}
	}
}
