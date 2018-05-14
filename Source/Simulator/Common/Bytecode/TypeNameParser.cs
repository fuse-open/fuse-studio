using Uno;
using Uno.Collections;

namespace Outracks.Simulator.Bytecode
{
	public class TypeNameParser
	{
		readonly List<string> _tokens;
		int _idx;

		public static TypeName Parse(string typeName)
		{
			return new TypeNameParser(TypeNameTokenizer.Tokenize(typeName)).ParseTypeName();
		}

		public TypeNameParser(List<string> tokens)
		{
			_tokens = tokens;
		}

		static List<TypeName> NoTypes { get { return new List<TypeName>();} }

		string Cur { get { return _tokens[_idx]; } }

		public TypeName ParseTypeName(Optional<TypeName> containingType = default(Optional<TypeName>))
		{
			var typeName = ParseSingleTypeName(containingType);
			ParsePossibleDot();
			if (_idx < _tokens.Count && !TypeNameTokenizer.IsSpecialChar(Cur[0]))
			{
				var tmp = ParseTypeName(typeName);
				return tmp;
			}
			return typeName;
		}

		TypeName ParseSingleTypeName(Optional<TypeName> containingType = default(Optional<TypeName>))
		{
			var typeName = new TypeName(containingType, _tokens[_idx++], ParsePossibleGenericArguments().ToImmutableList());
			return typeName;
		}

		List<TypeName> ParsePossibleGenericArguments()
		{
			var genericArugments = NoTypes;
			if (_idx >= _tokens.Count || !(Cur == "<"))
				return genericArugments;
			_idx++; //Skip '<'
			while (!(Cur == ">"))
			{
				genericArugments.Add(ParseTypeName());
				if (Cur == ",")
				{
					_idx++;
				}
			}
			_idx++; //Skip '>'
			return genericArugments;
		}

		void ParsePossibleDot()
		{
			if (_idx < _tokens.Count && Cur == ".")
				_idx++;
		}
	}
}