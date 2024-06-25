using System.Linq;
using System.Text;

namespace Outracks.Fusion.Windows
{
	static class FilterString
	{
		public static string CreateFilterString(this FileFilter[] filters)
		{
			if (filters == null || filters.Length == 0)
				return "All Files (*.*)|*.*";

			var sb = new StringBuilder();

			foreach (var f in filters)
			{
				if (sb.Length > 0)
					sb.Append('|');

				sb.Append(f.Label);

				sb.Append("|");

				//No need to fill in the parentheses according to MS examples. It does so automatically apparently.

				if (f.Extensions == null || f.Extensions.Length == 0)
				{
					sb.Append("*.*");
				}
				else
				{
					for (int i = 0; i < f.Extensions.Length; i++)
					{
						if (i > 0)
							sb.Append(";");

						sb.Append("*." + f.Extensions[i].Trim('.', '*'));
					}
				}
			}

			return sb.ToString();
		}

		public static FileFilter[] Parse(string filters)
		{
			var a = filters.Split("|");
			var b = a.Where((v, i) => i % 2 == 0);
			var c = a.Where((v, i) => i % 2 == 1);

			return b.Zip(c,
				(first, second) =>
					new FileFilter(first,
						second.Split(";").Select(s =>
							s.TrimStart('*', '.')
						).ToArray()
					)
				).ToArray();
		}


	}
}