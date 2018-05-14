using System.Reactive.Linq;

namespace Outracks
{
	public static partial class Property
	{
		public static IProperty<T> PreventFeedback<T>(this IProperty<T> self)
		{
			bool setByUs = false;
			return self.Where(_ => !setByUs).AsProperty(
				isReadOnly: self.IsReadOnly,
				write: (v, save) =>
				{
					setByUs = true;
					try
					{
						self.Write(v, save);
					}
					finally
					{
						setByUs = false;
					}
				});
		}
	}
}