using Uno;
using System.IO;

namespace Outracks.Simulator.Bytecode
{
	public class Variable : IEquatable<Variable>
	{

		public readonly string Name;

		public static readonly Variable This = new Variable("this");

		public Variable(string name)
		{
			Name = name;
		}

		public bool Equals(Variable other)
		{
			if (ReferenceEquals(null, other)) return false;
			if (ReferenceEquals(this, other)) return true;
			return String.Equals(Name, other.Name);
		}

		public override bool Equals(object obj)
		{
			if (ReferenceEquals(null, obj)) return false;
			if (ReferenceEquals(this, obj)) return true;
			return obj is Variable && Equals((Variable)obj);
		}

		public override int GetHashCode()
		{
			return (Name != null ? Name.GetHashCode() : 0);
		}

		public static bool operator ==(Variable left, Variable right)
		{
			return Equals(left, right);
		}

		public static bool operator !=(Variable left, Variable right)
		{
			return !Equals(left, right);
		}

		public override string ToString()
		{
			return Name;
		}

		public static readonly Action<Variable, BinaryWriter> Write = _Write; 

		public static void _Write( Variable v, BinaryWriter writer)
		{
			v._Write(writer);
		}

		public void _Write(BinaryWriter writer)
		{
			writer.Write(Name);
		}

		public static readonly Func<BinaryReader, Variable> Read = _Read; 

		public static Variable _Read(BinaryReader reader)
		{
			return new Variable(
				reader.ReadString());
		}

		public static string GetName(Variable arg)
		{
			return arg.Name;
		}
	}

}