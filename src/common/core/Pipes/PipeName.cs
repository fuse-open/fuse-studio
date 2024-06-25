
using System;
using System.IO;
using System.Runtime.Serialization;
using System.Security.Permissions;

namespace Outracks.IPC
{
	[Serializable]
	public struct PipeName : IEquatable<PipeName>, ISerializable
	{
		public static PipeName New()
		{
			return new PipeName(Guid.NewGuid());
		}
		public static Optional<PipeName> TryParse(string nativePipe)
		{
			return new PipeName(nativePipe);
		}

		readonly string _name;

		public PipeName(string name)
		{
			_name = name;
		}

		public PipeName(Guid guid)
		{
			_name = guid.ToString();
		}

		public static PipeName operator /(PipeName baseName, string subName)
		{
			return new PipeName(baseName + "." + subName);
		}

		public override string ToString()
		{
			return _name;
		}

		public override int GetHashCode()
		{
			return _name.GetHashCode();
		}

		public override bool Equals(object obj)
		{
			return obj is PipeName && Equals((PipeName)obj);
		}

		public bool Equals(PipeName other)
		{
			return _name == other._name;
		}

		public static bool operator ==(PipeName left, PipeName right)
		{
			return left.Equals(right);
		}

		public static bool operator !=(PipeName left, PipeName right)
		{
			return !left.Equals(right);
		}

		[SecurityPermission(SecurityAction.Demand, SerializationFormatter = true)]
		PipeName(SerializationInfo info, StreamingContext context)
		{
			_name = info.GetString("_name");
		}

		public void GetObjectData(SerializationInfo info, StreamingContext context)
		{
			if (info == null)
			{
				throw new ArgumentNullException("info");
			}

			info.AddValue("_name", _name);
		}

		public static PipeName Read(BinaryReader reader)
		{
			return new PipeName(reader.ReadString());
		}

		public static void Write(BinaryWriter writer, PipeName name)
		{
			writer.Write(name._name);
		}
	}
}