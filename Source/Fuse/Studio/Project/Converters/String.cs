namespace Outracks.Fuse
{
	public static class StringParser
	{
		public static IAttribute<string> GetString(this IElement element, string property, string defaultValue)
		{
			return element[property].Convert(
				parse: p => Parsed.Success(p, p),
				serialize: d => d,
				defaultValue: defaultValue);
		}
	}
}