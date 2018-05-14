using System;
using System.Collections.Generic;
using System.Linq;
using Outracks.Simulator.Bytecode;
using Outracks.Simulator.UXIL;
using Uno.UX.Markup;
using Uno.UX.Markup.UXIL;
using String = Uno.UX.Markup.String;

namespace Outracks.Simulator.CodeGeneration
{
	static class ExpressionGenerator
	{
		public static Expression GetValueExpression(this Property property, Context ctx)
		{
			return property.MatchWith(
				(BindableProperty p) => p.MatchWith(
					(ListProperty l) => Expression.Throw(new InvalidUXIL("Setting value of list property is illegal")),
					(ReferenceProperty r) => r.Source.GetExpression(ctx)),
				(AtomicProperty p) => p.Value.GetExpression(ctx),
				(DelegateProperty p) => Expression.Throw(new NotImplementedException("Setting value of delegate property not supported")));
		}

		public static Expression[] GetExpressions(this IEnumerable<ValueSource> args, Context ctx)
		{
			return args.Select(arg => arg.GetExpression(ctx)).ToArray();
		}

		public static Expression GetExpression(this ValueSource src, Context ctx)
		{
			return src.MatchWith(
				(ReferenceSource rs) => rs.GetExpression(ctx),
				(AtomicValueSource avs) => avs.Value.GetExpression(ctx));
		}

		public static Expression GetExpression(this ReferenceSource v, Context ctx)
		{
			return v.MatchWith(
				(BundleFileSource s) => s.GetExpression(ctx),
				(NodeSource s) => s.Node.GetExpression(ctx),
				(UXPropertySource s) => s.GetPropertyExpression(ctx),
				(UXPropertyAccessorSource s) => s.GetPropertyExpression(ctx));
		}

		public static Expression GetExpression(this Node node, Context ctx)
		{
			var resourceNode = node as ResourceRefNode;
			if (resourceNode != null)
			{
				var globalName = new Variable(resourceNode.StaticRefId);
				if (ctx.Names.Contains(globalName))
					return new ReadVariable(globalName);

				return new ReadStaticField(StaticMemberName.Parse(resourceNode.StaticRefId));
			}

			return new ReadVariable(ctx.Names[node]);
		}

		static Expression GetExpression(this GlobalReferenceValue grv, Context ctx)
		{
			if (grv.ResolvedValue != null)
				return grv.ResolvedValue.GetExpression(ctx);

			var globalName = new Variable(grv.ToLiteral());
			if (ctx.Names.Contains(globalName))
				return new ReadVariable(globalName);
			
			return new ReadStaticField(StaticMemberName.Parse(grv.ToLiteral()));
		}

		public static Expression GetExpression(this AtomicValue value, Context ctx)
		{
			return value.MatchWith(
				(Bool v) => new BooleanLiteral(v.Value),
				(EnumValue v) => new EnumLiteral(v.GetEnumValueName()),
				(ReferenceValue v) => v.Value.GetExpression(ctx),
				(String v) => new StringLiteral(v.Value), 
				(Scalar v) => v.GetExpression(),
				(UXIL.Vector v) => v.GetExpression(),
				(GlobalReferenceValue grv) => grv.GetExpression(ctx),
				(Uno.UX.Markup.Size v) => v.GetExpression(),
				(Size2 v) => v.GetExpression(),
				(Selector v) => v.GetExpression()); 
		}

		static Expression GetExpression(this Selector sel)
		{
			return new Instantiate(TypeName.Parse("Uno.UX.Selector"), new StringLiteral(sel.Value));
		}

		static Expression GetExpression(this UXIL.Vector vector)
		{
			return new Instantiate(vector.TypeName, vector.Components.Select(GetExpression).ToImmutableList());
		}

		static Expression GetExpression(this Uno.UX.Markup.Size s)
		{
			return new Instantiate(TypeName.Parse("Uno.UX.Size"), new NumberLiteral(NumberType.Float, s.Value), new EnumLiteral(new StaticMemberName(TypeName.Parse("Uno.UX.Unit"), new TypeMemberName(s.Unit))));
		}

		static Expression GetExpression(this Size2 s)
		{
			return new Instantiate(TypeName.Parse("Uno.UX.Size2"), GetExpression(s.X), GetExpression(s.Y));
		}

		static Expression GetExpression(this Scalar v)
		{
			var obj = v.ObjectValue;

			if (obj is double) return new NumberLiteral(NumberType.Double, (double) obj);
			if (obj is float) return new NumberLiteral(NumberType.Float, (float)obj);
			if (obj is int) return new NumberLiteral(NumberType.Int, (int)obj);
			if (obj is uint) return new NumberLiteral(NumberType.UInt, (uint)obj);
			if (obj is short) return new NumberLiteral(NumberType.Short , (short)obj);
			if (obj is ushort) return new NumberLiteral(NumberType.UShort, (ushort)obj);
			if (obj is sbyte) return new NumberLiteral(NumberType.SByte, (sbyte)obj);
			if (obj is byte) return new NumberLiteral(NumberType.Byte, (byte)obj);

			throw new Exception("Unknown scalar: " + v);
		}
	}
}