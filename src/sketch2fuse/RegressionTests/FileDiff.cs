using System;
using System.IO;

namespace RegressionTests
{
	public class FileDiff
	{
		public string OldFile => Path.Combine(OldDir, FileName);
		public string NewFile => Path.Combine(NewDir, FileName);
		public readonly string OldDir;
		public readonly string NewDir;
		public readonly string FileName;
		public readonly DiffType DiffType;
		public readonly string Diff;

		public FileDiff(string oldDir, string newDir, string fileName, DiffType diffType, string diff)
		{
			OldDir = oldDir;
			NewDir = newDir;
			FileName = fileName;
			DiffType = diffType;
			Diff = diff;
		}

		public string Summary
		{
			get
			{
				switch (DiffType)
				{
					case DiffType.Added:
						return "added file " + FileName;
					case DiffType.Modified:
						return "changed file " + FileName;
					case DiffType.Deleted:
						return "deleted file " + FileName;
					default:
						throw new NotImplementedException();
				}
			}
		}
	}

	public enum DiffType
	{
		Added,
		Deleted,
		Modified
	}
}
