using System.IO;
using Uno;
using Uno.Text;
using Uno.Collections;

namespace Outracks.Simulator.Bytecode
{
	public sealed partial class TypeName : IEquatable<TypeName>
	{
		public readonly Optional<TypeName> ContainingType;
		public readonly string Surname;
		public readonly ImmutableList<TypeName> GenericArguments;

		public TypeName(Optional<TypeName> containingType, string surname, ImmutableList<TypeName> genericArguments)
		{
			ContainingType = containingType;
			Surname = surname;
			GenericArguments = genericArguments;
		}

		public string FullName
        {
	        get
	        {
		        return (ContainingType.HasValue ? ContainingType.Value.FullName + "." : "") + Name;
	        }
        }

		public bool IsParameterizedGenericType

		{
			get { return GenericArguments.Count != 0 || (ContainingType.HasValue && ContainingType.Value.IsParameterizedGenericType); }
		}

		public TypeName WithGenericSuffix
		{
			get
			{
				return new TypeName(
					ContainingType.HasValue ? Optional.Some(ContainingType.Value.WithGenericSuffix) : Optional.None<TypeName>(),
					Surname + (GenericArguments.Count == 0 ? "" : "`" + GenericArguments.Count),
					ImmutableList<TypeName>.Empty);
			}
		}

		public ImmutableList<TypeName> GenericArgumentsRecursively
		{
			get
			{
				var args = new List<TypeName>();
				var typeName = this;
				while (typeName.ContainingType.HasValue)
				{
					args.AddRange(typeName.GenericArguments);
					typeName = typeName.ContainingType.Value;
				}
				return args.ToImmutableList();
			}
		}

		public string Name
		{
			get
			{
				return GenericArguments.Count != 0
					? Surname + "<" + GenericArguments.JoinToString(",") + ">"
					: Surname;
			}
		}

		public override string ToString()
		{
			return FullName;
		}

		public override int GetHashCode()
		{
			return FullName.GetHashCode();
		}

		public override bool Equals(object obj)
		{
			return obj is TypeName && Equals((TypeName)obj);
		}

		public bool Equals(TypeName other)
		{
			return FullName == other.FullName;
		}

		public static bool operator ==(TypeName a, TypeName b)
		{
			return a.Equals(b);
		}

		public static bool operator !=(TypeName a, TypeName b)
		{
			return !a.Equals(b);
		}

		public static Action<TypeName, BinaryWriter> Write = _Write;

		public static void _Write(TypeName name, BinaryWriter writer)
		{
			name._Write(writer);
		}

		public void _Write(BinaryWriter writer)
		{
			Optional.Write(writer, ContainingType, TypeName.Write);
			writer.Write(Surname);
			List.Write(writer, GenericArguments, TypeName.Write);
		}

		public static Func<BinaryReader, TypeName> Read = _Read;

		public static TypeName _Read(BinaryReader reader)
		{
			var containingType = Optional.Read(reader, TypeName.Read);
			var surname =reader.ReadString();
			var genericArguments =List.Read(reader, TypeName.Read);

			return new TypeName(
				containingType,
				surname,
				genericArguments);
		}

		public TypeName Parameterize(params TypeName[] methodArgumentTypes)
		{
			return new TypeName(ContainingType, Surname, ((IEnumerable<TypeName>)methodArgumentTypes).ToImmutableList());
		}

		public static TypeName Parse(string name)
		{
			return TypeNameParser.Parse(name);
		}

	}
}