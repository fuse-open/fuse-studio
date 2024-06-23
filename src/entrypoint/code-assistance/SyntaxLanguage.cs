using Outracks.CodeCompletion;

namespace Outracks.Fuse.CodeAssistance
{
	class SyntaxLanguage
	{
		string Type { get; set; }

		public SyntaxLanguageType Syntax { get; private set; }

		SyntaxLanguage(string type)
		{
			Type = type;
			Syntax = ParseTypeString(Type);
		}

		SyntaxLanguageType ParseTypeString(string type)
		{
			if(string.IsNullOrEmpty(type))
				return SyntaxLanguageType.Unknown;

			var loweredType = type.ToLower();
			switch (loweredType)
			{
				case "ux":
					return SyntaxLanguageType.UX;
				case "uno":
					return SyntaxLanguageType.Uno;
				default:
					return SyntaxLanguageType.Unknown;
			}
		}

		public static SyntaxLanguage FromString(string type)
		{
			return new SyntaxLanguage(type);
		}
	}
}