using Uno.Collections;
using System.IO;
using Uno;

namespace Outracks.Simulator.Bytecode
{
	public abstract class Statement
//		: IMatchTypes<
//			NoOperation,
//			BindVariable,
//			Expression,
//			Return>
	{
		public abstract char StatementId { get; }

		public T Match<T>(
			Func<NoOperation, T> a1,
			Func<Expression, T> a3,
			Func<Return, T> a4)
		{
			var self = this;
			var t1 = self as NoOperation; if (t1 != null) return a1(t1);
			var t3 = self as Expression; if (t3 != null) return a3(t3);
			var t4 = self as Return; if (t4 != null) return a4(t4);
			throw new NonExhaustiveMatch();
		}

		public void Match(
			Action<NoOperation> a1,
			Action<Expression> a3,
			Action<Return> a4)
		{
			var self = this;
			var t1 = self as NoOperation; if (t1 != null) { a1(t1); return; }
			var t3 = self as Expression; if (t3 != null) { a3(t3); return; }
			var t4 = self as Return; if (t4 != null) { a4(t4); return; }
			throw new NonExhaustiveMatch();
		}

		public static void Write(Statement statement, BinaryWriter writer)
		{
			writer.Write(statement.StatementId);
			statement.WriteStatement(writer);
		}

		protected abstract void WriteStatement(BinaryWriter writer);

		public static readonly Func<BinaryReader, Statement> Read = _Read;

		public static Statement _Read(BinaryReader reader)
		{
			var c = reader.ReadChar();
			switch (c)
			{
				case StatementIdRegistry.NoOperation: return NoOperation.Read(reader);
				case StatementIdRegistry.Expression: return Expression.Read(reader);
				case StatementIdRegistry.Return: return Return.Read(reader);
			}
			throw new InvalidDataException();
		}

		public static Statement Nop()
		{
			return new NoOperation("NOP");
		}

		public static Statement operator +(Statement left, Statement right)
		{
			return new CallLambda(
				new Lambda(
					Signature.Action(),
					ImmutableList<BindVariable>.Empty,
					List.Create<Statement>(left, right)));
		}

		public static IEnumerable<Statement> operator +(Statement statement, IEnumerable<Statement> statements)
		{
			return ((IEnumerable<Statement>)new[] { statement }).Union(statements);
		}

		public static IEnumerable<Statement> operator +(IEnumerable<Statement> statements, Statement statement)
		{
			return statements.Union(new[] { statement });
		}
	}
}