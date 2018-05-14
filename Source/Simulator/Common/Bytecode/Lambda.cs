using System;
using System.IO;
using Uno.Collections;
using Uno;
using Uno.Text;

namespace Outracks.Simulator.Bytecode
{
	public sealed class BindVariable
	{
		public readonly Variable Variable;
		public readonly Expression Expression;

		public BindVariable(Variable variable, Expression expression)
		{
			Variable = variable;
			Expression = expression;
		}

		public override string ToString()
		{
			return "var " + Variable.Name + " = " + Expression;
		}

		public static readonly Action<BindVariable, BinaryWriter> Write = _Write;

		public static void _Write(BindVariable s, BinaryWriter writer)
		{
			Variable.Write(s.Variable, writer);
			Expression.Write(s.Expression, writer);
		}

		public static readonly Func<BinaryReader, BindVariable> Read = _Read;

		public static BindVariable _Read(BinaryReader reader)
		{
			var variable = Variable.Read(reader);
			var expression = Expression.Read(reader);
			return new BindVariable(
				variable,
				expression);
		}
	}

	public sealed class Lambda : Expression
	{
		static int fguid = 0;


		public static Lambda Action(Func<Expression, Statement> body)
		{
			var a = new Variable("a" + fguid++);
			return new Lambda(
				Signature.Action(a),
				new BindVariable[0],
				new [] { body(new ReadVariable(a)) });
		}

		public static Lambda Func(Func<Expression, Statement> body)
		{
			var a = new Variable("a" + fguid++);
			return new Lambda(
				Signature.Func(TypeName.Parse("object"), a),
				new BindVariable[0],
				new[] { body(new ReadVariable(a)) });
		}

		public static Lambda Action(Func<Expression,Expression, Statement> body)
		{
			var a = new Variable("a" + fguid++);
			var b = new Variable("a" + fguid++);
			return new Lambda(
				Signature.Action(a,b),
				new BindVariable[0],
				new[] { body(
					new ReadVariable(a), 
					new ReadVariable(b)) });
		}

		public readonly Signature Signature;
		public readonly ImmutableList<BindVariable> LocalVariables;
		public readonly ImmutableList<Statement> Statements;

		public override char ExpressionId { get { return ExpressionIdRegistry.Lambda; } }

		public Lambda(
			Signature signature,
			IEnumerable<BindVariable> localVariables,
			IEnumerable<Statement> statements)
			: this(signature, localVariables.ToImmutableList(), statements.ToImmutableList())
		{ }

		public Lambda(
			Signature signature,
			ImmutableList<BindVariable> localVariables,
			ImmutableList<Statement> statements)
		{
			Signature = signature;
			LocalVariables = localVariables;
			Statements = statements;
		}

		public override string ToString()
		{
			var code = new StringBuilder();
			foreach (var stmnt in LocalVariables)
				code.Append(AddStatementSeparator(stmnt));
			foreach (var stmnt in Statements)
				code.Append(AddStatementSeparator(stmnt));

			return Signature + " => \n" +
				"{\n" +
					code.ToString().Indent() +
				"\n}";
		}

		static string AddStatementSeparator(object s)
		{
			return s + ";\n";
		}

		public static void Write(Lambda l, BinaryWriter writer)
		{
			Signature.Write(l.Signature, writer);
			List.Write(writer, l.LocalVariables, BindVariable.Write);
			List.Write(writer, l.Statements, Statement.Write);
		}

		protected override void WriteExpression(BinaryWriter writer)
		{
			Lambda.Write(this, writer);
		}

		new public static Lambda Read(BinaryReader reader)
		{
			var signature = Signature.Read(reader);
			var localVariables = List.Read(reader, BindVariable.Read);
			var statements = List.Read(reader, Statement.Read);
			return new Lambda(
				signature,
				localVariables,
				statements);
		}
	}
}