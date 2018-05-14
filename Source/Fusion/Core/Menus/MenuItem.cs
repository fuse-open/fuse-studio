using System;
using System.Reactive.Linq;
using System.Threading.Tasks;

namespace Outracks.Fusion
{
	public sealed class MenuItem
	{
		internal static MenuItem CreateSeparator()
		{
			return new MenuItem
			{
				Name = "-",
				Command = Command.Enabled(),
				Menu = Optional.None<Menu>(),
				IsToggled = Observable.Return(false),
				IsDefault = false,
				IsSeparator = true,
			};
		}

		internal static MenuItem Create(
			Text name,
			Optional<Command> command = default(Optional<Command>),
			HotKey hotkey = default(HotKey),
			IObservable<Optional<Icon>> icon = null,
			IObservable<bool> isToggled = null,
			bool isDefault = false)
		{
			return new MenuItem
			{
				Name = name,
				Command = command.Or(Command.Enabled()),
				Hotkey = hotkey,
				Icon = icon ?? Observable.Return(Optional.None<Icon>()),
				IsToggled = isToggled ?? Observable.Return(false),
				IsDefault = isDefault,
			};
		}

		internal static MenuItem Create(
			Text name,
			Menu menu,
			Optional<Icon> icon = default(Optional<Icon>))
		{
			return new MenuItem
			{
				Name = name,
				Command = Command.Enabled(),
				Hotkey = HotKey.None,
				Icon = Observable.Return(icon),
				Menu = Optional.Some(menu),
				IsToggled = Observable.Return(false),
				IsDefault = false
			};
		}

		MenuItem() { }

		public Text Name { get; private set; }

		public Command Command { get; private set; }

		public HotKey Hotkey { get; private set; }

		public IObservable<Optional<Icon>> Icon { get; private set; }

		public Optional<Menu> Menu { get; private set; }
	
		public bool IsDefault { get; private set; }

		public bool IsSeparator { get; private set; }
		
		public IObservable<bool> IsToggled { get; private set; }
	}
}