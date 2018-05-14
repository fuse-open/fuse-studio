using Uno.Collections;
using Uno;
using Uno.Text;

namespace Outracks.Simulator
{

	public static class IndentString
	{
		public static string Indent(this string str)
		{
			return str.Split('\n').Select((Func<string, string>)PrefixWithTab).Join("\n");
		}

		static string PrefixWithTab(string s)
		{
			return "\t" + s;
		}
	}

	public static class StringSplitting
	{
		public static int OrdinalLastIndexOf(this string str, string seperator)
		{
			for (int i = str.Length - seperator.Length; i-- > 0;)
			{
				int j = 0;
				while (j < seperator.Length && str[i + j] == seperator[j])
					j++;
				if (j == seperator.Length)
					return i;
			}
			return -1;
		}

		public static int OrdinalIndexOf(this string str, string seperator)
		{
			return str.IndexOf(seperator);
		}

		public static string AfterLast(this string s, string seperator)
		{
			var index = s.OrdinalLastIndexOf(seperator);
			if (index == -1)
				throw new Exception();
			return s.Substring(index + seperator.Length);
		}

		public static string BeforeLast(this string s, string seperator)
		{
			var index = s.OrdinalLastIndexOf(seperator);
			if (index == -1)
				throw new Exception();
			return s.Substring(0, index);

		}

		public static string AfterFirst(this string s, string seperator)
		{
			var index = s.IndexOf(seperator);
			if (index == -1)
				throw new Exception();
			return s.Substring(index + seperator.Length);
		}

		public static string BeforeFirst(this string s, string seperator)
		{
			var index = s.IndexOf(seperator);
			if (index == -1)
				throw new Exception();
			return s.Substring(0, index);

		}

		public static string JoinToString<T>(this IEnumerable<T> objects, string separator)
		{
            var sb = new StringBuilder();

	        var isFirst = true;
			foreach (var obj in objects)
	        {
		        if (isFirst)
			        isFirst = false;
				else
			        sb.Append(separator);
		        sb.Append(obj.ToString());
	        }

	        return sb.ToString();
		}

        public static string Join(this IEnumerable<string> si, string separator)
        {
            var sb = new StringBuilder();

	        var isFirst = true;
			foreach (var part in si)
	        {
		        if (isFirst)
			        isFirst = false;
				else
			        sb.Append(separator);
		        sb.Append(part);
	        }

	        return sb.ToString();
        }

    }

}