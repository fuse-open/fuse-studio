using System;

namespace Fuse.Preview
{
	public class Code
	{
		readonly string _code;

		public Code(string code)
		{
			_code = code;
		}

		public override string ToString()
		{
			return _code;
		}
	}

	class CodeGenerator
	{
		static readonly char []_tableData;

		static CodeGenerator()
		{
			_tableData = new char[26 + 10];
			var idx = 0;
			for (var i = 0; i < 26; ++i)
			{
				// Create alphabet
				_tableData[idx++] = (char)(i + 65);
			}

			for (var i = 0; i < 10; ++i)
			{
				// Create digits
				_tableData[idx++] = (char)(i + 48);
			}
		}

		public static Code CreateRandomCode(int numSymbols)
		{
			var code = "";
			var rand = new Random();
			for (var i = 0; i < numSymbols; ++i)
			{
				code += _tableData[rand.Next(_tableData.Length)];
			}
			return new Code(code);
		}
	}
}