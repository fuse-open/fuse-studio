using System.IO;
using Uno.Collections;
using Uno;
using Uno.Text;

namespace Outracks.Simulator.Bytecode
{

	public sealed class Signature
	{
		public static Signature Action(params Variable[] variables)
		{
			var parameters = new Parameter[variables.Length];
			for (int i = 0; i < variables.Length; i++)
				parameters[i] = new Parameter(TypeName.Parse("object"), variables[i]);
			return new Signature(new ImmutableList<Parameter>(parameters), Optional.None<TypeName>());
		}
		
		public static Signature Func(TypeName returnType, params Variable[] variables)
		{
			var parameters = new Parameter[variables.Length];
			for (int i = 0; i < variables.Length; i++)
				parameters[i] = new Parameter(TypeName.Parse("object"), variables[i]);
			return new Signature(new ImmutableList<Parameter>(parameters), Optional.Some(returnType));
		}

		public readonly ImmutableList<Parameter> Parameters;
		public readonly Optional<TypeName> ReturnType;

		public Signature(
			ImmutableList<Parameter> parameters,
			Optional<TypeName> returnType)
		{
			Parameters = parameters;
			ReturnType = returnType;
		}

		public static void Write(Signature s, BinaryWriter writer)
		{
			List.Write(writer, s.Parameters, Parameter.Write);
			Optional.Write(writer, s.ReturnType, TypeName.Write);
		}

		public static readonly Func<BinaryReader, Signature> Read = _Read;

		public static Signature _Read(BinaryReader reader)
		{
			var parameters = List.Read(reader, Parameter.Read);
			var returnType = Optional.Read(reader, TypeName.Read);
			return new Signature(
				parameters,
				returnType);
		}

		public override string ToString()
		{
			var names = new string[Parameters.Count];
			for (int i = 0; i < Parameters.Count; i++)
				names[i] = Parameters[i].ToString();

			return "(" + names.Join(", ") + ")";
		}

	}
}