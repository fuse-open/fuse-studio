using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using DiffPlex;
using DiffPlex.DiffBuilder;
using DiffPlex.DiffBuilder.Model;

namespace RegressionTests
{
	public static class DirectoryDiffer
	{
		public static List<FileDiff> Diff(string oldDir, string newDir)
		{
			AssertHasNoUnexpectedSubdirs(oldDir);
			AssertHasNoUnexpectedSubdirs(newDir);
			var diffs = new List<FileDiff>();
			if (Directory.Exists(oldDir))
			{
				foreach (var oldFile in Directory.GetFiles(oldDir))
				{
					var fileName = Path.GetFileName(oldFile);
					var newFile = Path.Combine(newDir, fileName);
					if (!File.Exists(newFile))
					{
						var sb = new StringBuilder();
						foreach (var line in File.ReadAllLines(oldFile))
						{
							sb.Append("-" + line + "\n");
						}
						diffs.Add(new FileDiff(oldDir, newDir, fileName, DiffType.Deleted, sb.ToString()));
					}
					else
					{
						var diffBuilder = new InlineDiffBuilder(new Differ());
						var diff = diffBuilder.BuildDiffModel(File.ReadAllText(oldFile), File.ReadAllText(newFile));
						if (diff.Lines.Any(l => l.Type == ChangeType.Inserted || l.Type == ChangeType.Deleted))
						{
							var sb = new StringBuilder();
							foreach (var line in diff.Lines)
							{
								switch (line.Type)
								{
									case ChangeType.Inserted:
										sb.Append("+");
										break;
									case ChangeType.Deleted:
										sb.Append("-");
										break;
									default:
										sb.Append(" ");
										break;
								}
								sb.Append(line.Text + "\n");
							}
							diffs.Add(new FileDiff(oldDir, newDir, fileName, DiffType.Modified, sb.ToString()));
						}
					}
				}
			}
			if (Directory.Exists(newDir))
			{
				foreach (var newFile in Directory.GetFiles(newDir))
				{
					var fileName = Path.GetFileName(newFile);
					var oldFile = Path.Combine(oldDir, fileName);
					if (!File.Exists(oldFile))
					{
						var sb = new StringBuilder();
						foreach (var line in File.ReadAllLines(newFile))
						{
							sb.Append("+" + line + "\n");
						}
						diffs.Add(new FileDiff(oldDir, newDir, fileName, DiffType.Added, sb.ToString()));
					}
				}
			}
			return diffs;
		}

		private static void AssertHasNoUnexpectedSubdirs(string dir)
		{
			if (Directory.Exists(dir) && Directory.GetDirectories(dir).Any(UnexpectedDir))
			{
				throw new Exception("A subdirectory exists in '" + dir + "', we don't support that.");
			}
		}

		private static readonly IEnumerable<string> ValidSubdirs = new[] { "images" };
		private static bool UnexpectedDir(string dir)
		{
			var index = dir.LastIndexOf(Path.DirectorySeparatorChar);
			var dirName = index == -1 ? dir : dir.Remove(0, index + 1);
			return !ValidSubdirs.Contains(dirName);
		}
	}

}
