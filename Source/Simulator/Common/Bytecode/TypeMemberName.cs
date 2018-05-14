using Uno;
using System.IO;

namespace Outracks.Simulator.Bytecode
{
	public sealed class TypeMemberName : IEquatable<TypeMemberName>
	{
		public readonly string Name;

		public TypeMemberName(string name)
		{
			if (name == null) throw new ArgumentNullException(name);
			Name = name;
		}

		public static StaticMemberName operator +(TypeName typeName, TypeMemberName memberName)
		{
			return new StaticMemberName(typeName, memberName);
		}

		public override bool Equals(object obj)
		{
			var other = obj as TypeMemberName;
			return other != null && Equals(other);
		}

		public bool Equals(TypeMemberName other)
		{
			return other.Name == Name;
		}

		public override int GetHashCode()
		{
			return Name.GetHashCode();
		}

		public override string ToString()
		{
			return Name;
		}

		public static TypeMemberName Read(BinaryReader reader)
		{
			return new TypeMemberName(reader.ReadString());
		}

		public static void Write(TypeMemberName name, BinaryWriter writer)
		{
			name.Write(writer);
		}

		public void Write(BinaryWriter writer)
		{
			writer.Write(Name);
		}
	}
}