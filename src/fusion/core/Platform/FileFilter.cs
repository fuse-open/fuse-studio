using System.Collections.Generic;

namespace Outracks
{
	public struct FileFilter
	{
		public readonly string Label;
		public readonly string[] Extensions;

		public FileFilter(string label, params string[] extensions)
		{
			Label = label;
			Extensions = extensions;
		}

		public static FileFilter Union(string label, FileFilter a, params FileFilter []sets)
		{
			var extensions = new List<string>();
			extensions.AddRange(a.Extensions);
			foreach (var set in sets)
			{
				extensions.AddRange(set.Extensions);
			}

			return new FileFilter(label, extensions.ToArray());
		}

		public override string ToString()
		{
			var extsString = "";
			var first = true;
			foreach(var s in Extensions){
				if(first) first = false;
				else extsString+=", ";
				extsString += s;
			}
			return "FileFilter(" + Label + ", " + extsString + ")";
		}
	}
}