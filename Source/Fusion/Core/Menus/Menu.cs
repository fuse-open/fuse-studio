using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using System.Reactive.Linq;

namespace Outracks.Fusion
{
	public struct Menu : IEquatable<Menu>
	{
		public static readonly Menu Empty = new Menu(ObservableList.Empty<MenuItem>());

		public static Menu operator +(Menu top, Menu bottom)
		{
			return top._items.Match(
				topList => bottom._items.Match(
					bottomList => new Menu(topList.AddRange(bottomList)),
					bottomObsList => new Menu(top.Items.Concat(bottomObsList))),
				topObsList => new Menu(topObsList.Concat(bottom.Items)));
		}

		public static Menu Separator
		{
			get { return FromItem(MenuItem.CreateSeparator()); }
		}

		public static Menu Item(
			Text name = default(Text),
			Action action = null,
			IObservable<bool> isEnabled = null,
			HotKey hotkey = default(HotKey),
			Optional<Icon> icon = default(Optional<Icon>),
			bool isDefault = false)
		{
			return Item(name: name, command: Command.Create(isEnabled ?? Observable.Return(true), action), hotkey: hotkey, icon: icon, isDefault: isDefault);
		}

		public static Menu Item(
			Text name = default(Text),
			Optional<Command> command = default(Optional<Command>),
			HotKey hotkey = default(HotKey),
			Optional<Icon> icon = default(Optional<Icon>),
			bool isDefault = false)
		{
			return FromItem(MenuItem.Create(name: name, command: command, hotkey: hotkey, icon: Observable.Return(icon), isDefault: isDefault));
		}

		public static Menu Toggle(
			Text name = default(Text),
			IProperty<bool> toggle = null,
			HotKey hotkey = default(HotKey),
			Optional<Icon> icon = default(Optional<Icon>),
			IObservable<bool> isEnabled = null)
		{
			toggle = toggle ?? Property.Create(false);
			return FromItem(MenuItem.Create(name: name, command: toggle.Toggle(), hotkey: hotkey, isToggled: toggle));
		}

		public static Menu Option<T>(
			T value,
			Text name = default(Text),
			IProperty<T> property = null,
			HotKey hotkey = default(HotKey),
			IObservable<bool> isEnabled = null)
		{
			property = property ?? Property.Create(value);
			return FromItem(MenuItem.Create(
					name: name,
					command: Command.Enabled(() => property.Write(value)),
					hotkey: hotkey,
					isToggled: property.Select(p => p.Equals(value))));
		}

		public static Menu Submenu(
			Text name,
			Menu submenu,
			Optional<Icon> icon = default(Optional<Icon>))
		{
			return FromItem(MenuItem.Create(name: name, menu: submenu, icon: icon));
		}

		static Menu FromItem(MenuItem item)
		{
			return new Menu(ImmutableList.Create(item));
		}

		internal Menu(IObservableList<MenuItem> items)
		{
			_items = new Either<ImmutableList<MenuItem>, IObservableList<MenuItem>>(items);
		}

		internal Menu(ImmutableList<MenuItem> items)
		{
			_items = new Either<ImmutableList<MenuItem>, IObservableList<MenuItem>>(items);
		}

		readonly Either<ImmutableList<MenuItem>, IObservableList<MenuItem>> _items;

		public IObservableList<MenuItem> Items
		{
			get
			{
				return _items == null
					? Empty.Items
					: _items.Match(ObservableList.Return, ol => ol);
			}
		}

		public override bool Equals(object o)
		{
			return o is Menu && Equals((Menu)o);
		}

		public bool Equals(Menu other)
		{
			return Items == other.Items;
		}

		public override int GetHashCode()
		{
			return Items.GetHashCode();
		}

		public static bool operator ==(Menu lhs, Menu rhs)
		{
			return lhs.Equals(rhs);
		}

		public static bool operator !=(Menu lhs, Menu rhs)
		{
			return !lhs.Equals(rhs);
		}
	}

	public static class MenuExtensions
	{
		public static Menu ConnectWhile(this Menu self, IObservable<bool> condition)
		{
			return new Menu(self.Items.ConnectWhile(condition));
		}

		public static Menu ShowWhen(this Menu self, IObservable<bool> condition)
		{
			return new Menu(condition.Switch(c => c ? self.Items : Menu.Empty.Items));
		}

		public static Menu Switch(this IObservable<Menu> menu)
		{
			return new Menu(menu.Switch(m => m.Items));
		}

		public static Menu Concat(this IEnumerable<Menu> menus)
		{
			return menus.Aggregate(Menu.Empty, (m1, m2) => m1 + m2);
		}

		public static Menu Concat(this IObservable<IEnumerable<Menu>> obsMenus)
		{
			return obsMenus.Select(Concat).Switch();
		}
	}
}