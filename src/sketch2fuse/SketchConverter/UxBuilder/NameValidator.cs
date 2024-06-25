using System.Linq;
using System.Text.RegularExpressions;

namespace SketchConverter.UxBuilder
{
	public static class NameValidator
	{
		public static NameValidity NameIsValid(string name)
		{
			if (!ValidCharacters.IsMatch(name))
				return NameValidity.InvalidCharacter;
			if (UnoKeywords.Contains(name))
				return NameValidity.InvalidKeyword;
			if (CSharpKeywords.Contains(name))
				return NameValidity.InvalidKeyword;
			return NameValidity.Valid;
		}

		private static readonly Regex ValidCharacters = new Regex("^[A-Za-z_][A-Za-z0-9_]*$", RegexOptions.Compiled);

		//Taken from https://github.com/fusetools/uno/blob/0a1f0982cf0048937bd934bf7b1839599c0663bf/src/compiler/Uno.Compiler.Frontend/Analysis/Tokens.cs#L15
		private static readonly string[] UnoKeywords = {
			"add", "apply", "block", "draw", "draw_dispose", "drawable", "get", "global", "immutable", "interpolate",
			"intrinsic", "local", "meta", "norm", "partial", "pixel", "pixel_sampler", "prev", "remove", "req", "sample", "set",
			"swizzler", "tag", "undefined", "var", "vertex", "vertex_attrib", "vertex_texture", "where", "yield"
		};

		//Taken from https://docs.microsoft.com/en-us/dotnet/csharp/language-reference/keywords/
		private static readonly string[] CSharpKeywords = {
			"abstract", "add", "alias", "as", "ascending", "async", "await", "base", "bool", "break", "byte", "case", "catch",
			"char", "checked", "class", "const", "continue", "decimal", "default", "delegate", "descending", "do", "double",
			"dynamic", "else", "enum", "event", "explicit", "extern", "false", "finally", "fixed", "float", "for", "foreach",
			"from", "get", "global", "goto", "group", "if", "implicit", "in", "int", "interface", "internal", "into", "is",
			"join", "let", "lock", "long", "nameof", "namespace", "new", "null", "object", "operator", "orderby", "out",
			"override", "params", "partial", "private", "protected", "public", "readonly", "ref", "remove", "return", "sbyte",
			"sealed", "select", "set", "short", "sizeof", "stackalloc", "static", "static", "string", "struct", "switch", "this",
			"throw", "true", "try", "typeof", "uint", "ulong", "unchecked", "unsafe", "ushort", "using", "using", "value", "var",
			"virtual", "void", "volatile", "when", "where", "while", "yield"
		};
	}

	public enum NameValidity
	{
		Valid,
		InvalidCharacter,
		InvalidKeyword
	}
}
