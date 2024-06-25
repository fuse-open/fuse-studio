using System.Collections.Generic;
using Outracks.IO;

namespace Outracks.Fusion
{
	public static class UserSettings
	{
		public static ISettings Settings { get; set; }

		public static IProperty<Optional<bool>> Bool(string name)
		{
			return Settings.Property<bool>(name);
		}

		public static IProperty<Optional<Size<Points>>> Size(string name)
		{
			return Settings.Property<Size<Points>>(name);
		}

		public static IProperty<Optional<Points>> Point(string name)
		{
			return Settings.Property<Points>(name);
		}

		public static IProperty<Optional<Point<Points>>> Position(string name)
		{
			return Settings.Property<Point<Points>>(name);
		}

		public static IProperty<Optional<IEnumerable<T>>> List<T>(string name)
		{
			return Settings.Property<IEnumerable<T>>(name);
		}

		public static IProperty<Optional<AbsoluteDirectoryPath>> Folder(string name)
		{
			return Settings.Property<AbsoluteDirectoryPath>(name);
		}

		public static IProperty<Optional<T>> Enum<T>(string name)
		{
			return Settings.Property<T>(name);
		}
	}
}