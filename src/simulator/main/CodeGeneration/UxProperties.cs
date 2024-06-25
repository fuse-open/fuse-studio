using Outracks.Simulator.Bytecode;
using Outracks.Simulator.UXIL;
using Uno.UX.Markup.Reflection;
using Uno.UX.Markup.UXIL;

namespace Outracks.Simulator.CodeGeneration
{
	static class PropertyExpression
	{
		public static Expression GetPropertyExpression(this UXPropertyAccessorSource s, Context ctx)
		{
			var property = s.Property;
			return GetPropertyExpression(ctx, property, Optional.None());
		}

		public static Expression GetPropertyExpression(this UXPropertySource s, Context ctx)
		{
			var obj = s.Node.GetExpression(ctx);
			var property = s.Property.Property;
			return GetPropertyExpression(ctx, property, obj);
		}

		static Expression GetPropertyExpression(Context ctx, Property property, Optional<Expression> maybeObj)
		{
			var propertyName = property.GetMemberName();
			var propertyType = property.Facet.DataType.GetTypeName();

			var vars = ctx.Names;

			var propObj = vars.GetUniqueName();
			vars = vars.Reserve(propObj);

			var value = vars.GetUniqueName();
			vars = vars.Reserve(value);

			var origin = vars.GetUniqueName();
			vars = vars.Reserve(origin);

			if (ctx.IsDeclaredInUx(property))
			{
				var setter = new Lambda(
					signature: Signature.Action(propObj, value, origin),
					localVariables: new BindVariable[0],
					statements: new Statement[]
					{
						new CallStaticMethod(
							StaticMemberName.Parse("Uno.UX.SimulatedProperties.Set"),
							new ReadVariable(propObj),
							new StringLiteral(propertyName.Name),
							new ReadVariable(value),
							new ReadVariable(origin))
					});

				var getter = new Lambda(
					signature: new Signature(List.Create(new Parameter(TypeName.Parse("object"), propObj)), propertyType),
					localVariables: new BindVariable[0],
					statements: new Statement[]
					{
						new Return(
							new CallStaticMethod(
								StaticMemberName.Parse("Uno.UX.SimulatedProperties.Get"),
								new ReadVariable(propObj),
								new StringLiteral(propertyName.Name))),
					});

				var mp = (IMutableProperty)property.Facet;

				return new Instantiate(
					UxPropertyType.Parameterize(propertyType),
					setter,
					getter,
					maybeObj.Or((Expression)new StringLiteral(null)),
					// the only null we've got is string
					new StringLiteral(propertyName.Name),
					new BooleanLiteral(mp.OriginSetterName != null)
					);
			}
			else
			{
				var obj = maybeObj.Or((Expression)new ReadVariable(propObj));

				var setter = new Lambda(
					signature: Signature.Action(propObj, value, origin),
					localVariables: new BindVariable[0],
					statements: new[]
					{
						property.SetValueStatement(ctx, obj, new ReadVariable(value), new ReadVariable(origin))
					});

				var getter = new Lambda(
					signature: new Signature(List.Create(new Parameter(TypeName.Parse("object"), propObj)), propertyType),
					localVariables: new BindVariable[0],
					statements: new[]
					{
						new Return(property.GetValueStatement(ctx, obj))
					});

				var mp = (IMutableProperty)property.Facet;

				return new Instantiate(
					UxPropertyType.Parameterize(propertyType),
					setter,
					getter,
					maybeObj.Or((Expression)new StringLiteral(null)),
					// the only null we've got is string
					new StringLiteral(propertyName.Name),
					new BooleanLiteral(mp.OriginSetterName != null)
					);
			}
		}

		public static Statement SetValueStatement(this Property property, Context ctx, Expression obj, Expression value, Expression origin)
		{
			var mp = (IMutableProperty)property.Facet;
			return mp.OriginSetterName != null
				? (Statement)new CallDynamicMethod(obj, new TypeMemberName(mp.OriginSetterName), value, origin)
				: property.IsAttachedProperty()
					? (Statement)new CallStaticMethod(property.GetSetMethodName(), obj, value)
					: (Statement)new WriteProperty(obj, property.GetMemberName(), value);
		}

		public static Expression GetValueStatement(this Property property, Context ctx, Expression obj)
		{
			return property.IsAttachedProperty()
				? (Expression)new CallStaticMethod(property.GetGetMethodName(), obj)
				: (Expression)new ReadProperty(obj, property.GetMemberName());
		}
		static TypeName UxPropertyType
		{
			get { return TypeName.Parse("Outracks.Simulator.Runtime.UxProperty<T>"); }
		}
	}
}