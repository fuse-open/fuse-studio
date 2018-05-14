using Uno;
using Uno.Collections;

namespace Outracks.Simulator.Bytecode
{
	public class TypeNameTokenizer
	{
		readonly string _name;
		int _idx;
		readonly List<string> _tokens = new List<string>();

		public static List<string> Tokenize(string name)
		{
			var tokenizer = new TypeNameTokenizer(name);
			tokenizer.Tokenize();
			return tokenizer._tokens;
		}

		public static bool IsSpecialChar(char c)
		{
			return c == '.' || c == '<' || c == '>' || c == ',';
		}

		TypeNameTokenizer(string name)
		{
			_name = name;
		}

		void Tokenize()
		{
			while (_idx < _name.Length)
			{
				switch (_name[_idx]) {
					case '.':
						_tokens.Add(".");
						_idx++;
						break;
					case '<':
						_tokens.Add("<");
						_idx++;
						break;
					case '>':
						_tokens.Add(">");
						_idx++;
						break;
					case ',':
						_tokens.Add(",");
						_idx++;
						break;
					default:
						ReadName();
						break;
				}
			}
		}

		void ReadName()
		{
			var end = _idx + 1;
			while (end < _name.Length && !IsSpecialChar(_name[end]))
			{
				end++;
			}
			_tokens.Add(_name.Substring(_idx, end - _idx));
			_idx = end;
		}
	}
}