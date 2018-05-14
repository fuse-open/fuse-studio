using System.Reactive.Linq;
using Outracks.Fusion;

namespace Outracks.Fuse
{
	public class ThemedTextBox
	{
		public static IControl Create(IProperty<string> editString, bool doWrap = false)
		{
			var isReadOnly = editString.IsReadOnly.Replay(1).RefCount();

			return TextBox.Create(
					text: editString,
					foregroundColor: isReadOnly.Select(
						ro => ro 
							? Theme.DescriptorText  // this looks correct, but maybe there should be an "InactiveText" property in theme too? */
							: Theme.DefaultText)
						.Switch(),
					doWrap: doWrap)
				.WithPadding(new Thickness<Points>(6, 2))
				.WithBackground(isReadOnly.Select(ro => ro ? Brush.Transparent : Theme.FieldBackground).Switch())
				.WithOverlay(Shapes.Rectangle(stroke: Theme.FieldStroke))
				.WithHeight(21)
				.CenterVertically();
		}
	}
}