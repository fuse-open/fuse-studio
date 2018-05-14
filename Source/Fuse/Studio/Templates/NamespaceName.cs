using System;

namespace Outracks.Fuse.Templates
{
	public sealed class NamespaceName : IEquatable<NamespaceName>
	{
		public readonly string FullName;
		public NamespaceName(string fullName)
		{
			FullName = fullName;
		}

		public bool Equals(NamespaceName other)
		{
			if (ReferenceEquals(null, other)) return false;
			if (ReferenceEquals(this, other)) return true;
			return string.Equals(FullName, other.FullName);
		}

		public override bool Equals(object obj)
		{
			if (ReferenceEquals(null, obj)) return false;
			if (ReferenceEquals(this, obj)) return true;
			return obj is NamespaceName && Equals((NamespaceName)obj);
		}

		public override int GetHashCode()
		{
			return (FullName != null ? FullName.GetHashCode() : 0);
		}

		public static bool operator ==(NamespaceName left, NamespaceName right)
		{
			return Equals(left, right);
		}

		public static bool operator !=(NamespaceName left, NamespaceName right)
		{
			return !Equals(left, right);
		}
	}
}