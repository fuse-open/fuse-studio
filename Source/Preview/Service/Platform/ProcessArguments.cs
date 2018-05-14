using System.Collections.Generic;
using System.Linq;
using Outracks;

namespace Fuse.Preview
{
	static class ProcessArguments
	{
		public static IEnumerable<string> UnpackList(string arguments)
		{
			var characters = new List<char>();

			bool isQuoted = false;
			for (int i = 0; i < arguments.Length; i++)
			{
				var c = arguments[i];
				if (c == '"')
				{
					isQuoted = characters.Count == 0;
				}
				else if (c == ' ' && !isQuoted && characters.Count > 0)
				{
					yield return new string(characters.ToArray());
					characters.Clear();
				}
				else
				{
					characters.Add(c);
				}
			}

			if (characters.Count > 0)
				yield return new string(characters.ToArray());
		}

		public static string PackList(IEnumerable<string> arguments)
		{
			return arguments.Select(a => "\"" + a + "\"").Join(" ");
		}
	}
}