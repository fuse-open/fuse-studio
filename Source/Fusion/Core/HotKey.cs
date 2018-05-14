using System;

namespace Outracks.Fusion
{
	public struct HotKey : IEquatable<HotKey>
	{
		public static HotKey None
		{
			get { return default(HotKey); }
		}

		public static HotKey Create(ModifierKeys modifier, Key key)
		{
			return new HotKey
			{
				Modifier = modifier,
				Key = key,
			};
		}

		public ModifierKeys Modifier { get; private set; }
		public Key Key { get; private set; }

		public override string ToString()
		{
			return Modifier != ModifierKeys.None
				? Modifier + " " + Key
				: Key.ToString();
		}
		
		public bool Equals(HotKey other)
		{
			return Modifier == other.Modifier && Key == other.Key;
		}

		public override bool Equals(object obj)
		{
			if (ReferenceEquals(null, obj)) return false;
			return obj is HotKey && Equals((HotKey)obj);
		}

		public override int GetHashCode()
		{
			unchecked
			{
				return ((int)Modifier * 397) ^ (int)Key;
			}
		}

		public static bool operator ==(HotKey left, HotKey right)
		{
			return left.Equals(right);
		}

		public static bool operator !=(HotKey left, HotKey right)
		{
			return !left.Equals(right);
		}

	}
}