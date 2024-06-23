
using Uno;
using System.IO;

namespace Outracks.Simulator.Bytecode
{
	public class MethodGroup : Expression
	{
		public readonly Expression Object;
		public readonly TypeMemberName MethodName;
		public readonly Optional<Signature> MethodSignature;
		public readonly TypeName DelegateType;

		public override char ExpressionId { get { return ExpressionIdRegistry.MethodGroup; } }

		public MethodGroup(Expression o, TypeMemberName methodName, Optional<Signature> methodSignature, TypeName delegateType)
		{
			Object = o;
			MethodName = methodName;
			MethodSignature = methodSignature;
			DelegateType = delegateType;
		}

		public override string ToString()
		{
			return "((" + DelegateType + ")(" + Object + ")." + MethodName + ")";
		}

		public static void Write(MethodGroup l, BinaryWriter writer)
		{
			Expression.Write(l.Object, writer);
			TypeMemberName.Write(l.MethodName, writer);
			Optional.Write(writer, l.MethodSignature, Signature.Write);
			TypeName.Write(l.DelegateType, writer);
		}

		protected override void WriteExpression(BinaryWriter writer)
		{
			MethodGroup.Write(this, writer);
		}

		new public static MethodGroup Read(BinaryReader reader)
		{
			var a = Expression.Read(reader);
			var b = TypeMemberName.Read(reader);
			var c = Optional.Read(reader, Signature.Read);
			var d = TypeName.Read(reader);
			return new MethodGroup(
				a,
				b,
				c,
				d);
		}
	}
}