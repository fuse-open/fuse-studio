using System.IO;
using Uno.Collections;
using Uno;
using Uno.Text;

namespace Outracks.Simulator.Bytecode
{
	public sealed class Parameter
	{
		public readonly TypeName Type;
		public readonly Variable Name;

		public Parameter(
			TypeName type,
			Variable name)
		{
			Type = type;
			Name = name;
		}

		public static Func<BinaryReader, Parameter> Read = _Read;

		static Parameter _Read(BinaryReader reader)
		{
			var type = TypeName.Read(reader);
			var name = Variable.Read(reader);
			return new Parameter(type, name);
		}

		public static Action<Parameter, BinaryWriter> Write = _Write;

		static void _Write(Parameter p, BinaryWriter writer)
		{
			TypeName.Write(p.Type, writer);
			Variable.Write(p.Name, writer);
		}

		public override string ToString()
		{
			return Type + " " + Name;
		}
	}
}