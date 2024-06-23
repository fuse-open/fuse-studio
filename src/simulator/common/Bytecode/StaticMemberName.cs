using Uno;
using System.IO;

namespace Outracks.Simulator.Bytecode
{
	public sealed class StaticMemberName : IEquatable<StaticMemberName>
	{
		public static StaticMemberName Parse(string value)
		{
			return new StaticMemberName(
				TypeName.Parse(value.BeforeLast(".")),
				new TypeMemberName(value.AfterLast(".")));
		}

		public readonly TypeName TypeName;
		public readonly TypeMemberName MemberName;

		public StaticMemberName(TypeName typeName, TypeMemberName memberName)
		{
			TypeName = typeName;
			MemberName = memberName;
		}

		public bool Equals(StaticMemberName other)
		{
			if (ReferenceEquals(null, other)) return false;
			if (ReferenceEquals(this, other)) return true;
			return TypeName.Equals(other.TypeName) && Equals(MemberName, other.MemberName);
		}

		public override bool Equals(object obj)
		{
			if (ReferenceEquals(null, obj)) return false;
			if (ReferenceEquals(this, obj)) return true;
			return obj is StaticMemberName && Equals((StaticMemberName)obj);
		}

		public override int GetHashCode()
		{
			unchecked
			{
				return (TypeName.GetHashCode() * 397) ^ (MemberName != null ? MemberName.GetHashCode() : 0);
			}
		}

		public static bool operator ==(StaticMemberName left, StaticMemberName right)
		{
			return Equals(left, right);
		}

		public static bool operator !=(StaticMemberName left, StaticMemberName right)
		{
			return !Equals(left, right);
		}

		public override string ToString()
		{
			return TypeName.FullName + "." + MemberName.Name;
		}

		public static void Write(StaticMemberName m, BinaryWriter writer)
		{
			TypeName.Write(m.TypeName, writer);
			TypeMemberName.Write(m.MemberName, writer);
		}

		public static StaticMemberName Read(BinaryReader reader)
		{
			var typeName =TypeName.Read(reader);
			var memberName =TypeMemberName.Read(reader);
			return new StaticMemberName(
				typeName,
				memberName);
		}
	}
}