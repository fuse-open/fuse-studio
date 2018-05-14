using System;
using System.Diagnostics.Contracts;
using System.Globalization;

namespace Outracks.Fuse.Protocol
{
	public class DaemonKey : IEquatable<DaemonKey>
	{
		public bool Equals(DaemonKey other)
		{
			if (ReferenceEquals(null, other)) return false;
			if (ReferenceEquals(this, other)) return true;
			return Key.Equals(other.Key);
		}

		public override bool Equals(object obj)
		{
			if (ReferenceEquals(null, obj)) return false;
			if (ReferenceEquals(this, obj)) return true;
			if (obj.GetType() != this.GetType()) return false;
			return Equals((DaemonKey) obj);
		}

		public override int GetHashCode()
		{
			return Key.GetHashCode();
		}

		public static bool operator ==(DaemonKey left, DaemonKey right)
		{
			return Equals(left, right);
		}

		public static bool operator !=(DaemonKey left, DaemonKey right)
		{
			return !Equals(left, right);
		}

		public readonly string Key;

		public DaemonKey(string key)
		{
			Key = key;
		}

		public string Serialize()
		{
			return Key;
		}

		public static DaemonKey Deserialize(string data)
		{
			return new DaemonKey(data);
		}

		[Pure]
		public override string ToString()
		{
			return Key.ToString(CultureInfo.InvariantCulture);
		}

		public static DaemonKey GetDaemonKey()
		{
			// We have it here until, we have optimized fuse loading.
			return new DaemonKey(Environment.UserName);
		}
	}
}
