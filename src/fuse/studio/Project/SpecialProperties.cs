
using Outracks.Fusion;

namespace Outracks.Fuse
{
	public static class SpecialProperties
	{
		public static IAttribute<string> UxName(this IElement element)
		{
			return element.GetString("ux:Name", "");
		}

		public static IProperty<Optional<string>> UxKey(this IElement element)
		{
			return element["ux:Key"];
		}

		public static IProperty<Optional<string>> UxProperty(this IElement element)
		{
			return element["ux:Property"];
		}

		public static IProperty<Optional<string>> UxValue(this IElement element)
		{
			return element["ux:Value"];
		}

		public static IProperty<Optional<string>> UxClass(this IElement element)
		{
			return element["ux:Class"];
		}

		public static IBehaviorProperty<Optional<string>> UxClass(this ILiveElement element)
		{
			return element["ux:Class"];
		}

		public static IProperty<Optional<string>> UxInnerClass(this IElement element)
		{
			return element["ux:InnerClass"];
		}

		public static IProperty<Optional<string>> UxGlobal(this IElement element)
		{
			return element["ux:Global"];
		}

	}
}
