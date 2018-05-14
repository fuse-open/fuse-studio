using System.Collections.Generic;

namespace Outracks.Fuse
{
	public static class ArgumentParseExtensions
	{
		public static Optional<string> TryParse(this List<string> args, string optionName)
		{
			var argIdx = args.FindIndex(s => s.StartsWith("--" + optionName));
			if (argIdx >= 0)
			{
				var result = args[argIdx]
					.Split("=")[1]
					.Trim('"');
				args.RemoveAt(argIdx);
				return result;
			}

			return Optional.None();
		}
	}
}