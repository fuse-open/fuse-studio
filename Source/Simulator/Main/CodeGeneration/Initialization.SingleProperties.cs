using System;
using System.Collections.Generic;
using System.Linq;
using Outracks.Simulator.Protocol;
using Uno.UX.Markup;
using Uno.UX.Markup.UXIL;
using Property = Uno.UX.Markup.UXIL.Property;

namespace Outracks.Simulator.CodeGeneration
{
	using Bytecode;
	using UXIL;

	public class SingleProperties
	{
		readonly Node _self;
		readonly Context _ctx;
		readonly Expression _selfExpression;
		
		public SingleProperties(Node self, Context ctx, Expression selfExpression = null)
		{
			_self = self;
			_ctx = ctx;
			_selfExpression = selfExpression ?? new ReadVariable(_ctx.Names[_self]);
		}

		public IEnumerable<Statement> InitializeValues()
		{
			return _self
				.SinglePropertiesWithValues()
				.Select(InitializeValue);
		}

		Statement InitializeValue(Property property)
		{
			return UpdateValue(property, property.GetValueExpression(_ctx));
		}

		public Statement UpdateValue(string propertyName, Optional<string> maybeValue, ValueParser valueParser)
		{
			foreach (var p in _self.Properties)
			{
				if (p.Facet.Name == propertyName || p.Facet.Name.EndsWith("."+propertyName))
					return UpdateValue(p, maybeValue, valueParser);
			}

			throw new Exception("Property not found: " + propertyName);
		}

		Statement UpdateValue(Property property, Optional<string> maybeValue, ValueParser valueParser)
		{
			// TODO: we should ensure that the bytecode is evaluated in the declaring environment of the object, until then reference properties can't be set
			// It could be that it's actually false that AtomicValue can be reference type, but hard to know
			// I'll just avoid setting reference properties i guess
			// ..I can't know what is a reference property in the tool, i should probably trigger reify from the simulator in response to setting the property


			return property.MatchWith(
				(BindableProperty bp) =>
				{

					if (bp.BindableType.FullName == "Fuse.Drawing.Brush")
					{
						var solidColorBrush = TypeName.Parse("Fuse.Drawing.StaticSolidColor");
						var float4 = TypeName.Parse("float4");
						return maybeValue
							.Select(value => valueParser.Parse(value, float4, FileSourceInfo.Unknown))
							.MatchWith(
								some: v => UpdateValue(property, new Instantiate(solidColorBrush, v.GetExpression(_ctx))),
								none: () => ResetProperty(property));
					}

					throw new ReifyRequired();
				},
				(AtomicProperty p) => maybeValue
					.Select(value => valueParser.Parse(value, property.Facet.DataType, FileSourceInfo.Unknown))
					.MatchWith(
						some: v => UpdateValue(property, v.GetExpression(_ctx)),
						none: () => ResetProperty(property)),
				(DelegateProperty dp) =>
				{
					throw new ReifyRequired();
				});
		}

		Statement ResetProperty(Property p)
		{
			//var resetMethod = p.GetResetMethod();
			//if (resetMethod.HasValue)
			//	return new CallStaticMethod(resetMethod.Value, _selfExpression);

			//// It's all a lie
			
			//if (p.Facet.DataType.FullName == "float" && p.Facet.Name == "Opacity")
			//	return UpdateValue(p, new NumberLiteral(NumberType.Float, 1.0));

			//var knownBrushProperties = new[]
			//{
			//	"Background"
			//};
			//if (p.Facet.DataType.FullName == "Fuse.Drawing.Brush" && knownBrushProperties.Contains(p.Facet.Name))
			//	return UpdateValue(p, new StringLiteral(null));

			//var knownValueTypes = new[]
			//{
			//	"Fuse.Elements.Alignment", 
			//	"Fuse.Layouts.Dock", 
			//	"Fuse.Controls.TextAlignment", 
			//	"Fuse.Layouts.Orientation",
			//	"Uno.UX.Size", "Uno.UX.Size2", 
			//	"float", "float2", "float4",
			//};
			//if (knownValueTypes.Contains(p.Facet.DataType.FullName))
			//	return UpdateValue(p, new Instantiate(TypeName.Parse(p.Facet.DataType.FullName)));

			throw new ReifyRequired(); 
		}

		Statement UpdateValue(Property prop, Expression value)
		{
			var setArgs = new[] { _selfExpression, value };

			if (prop.IsAttachedProperty())
			{
				var setMethod = prop.GetSetMethodName();

				return new CallStaticMethod(setMethod, setArgs);
			}
			
			if (_ctx.IsDeclaredInUx(prop))
			{
				var nameString = new StringLiteral(prop.GetMemberName().Name);
				var nameSelector = new Instantiate(TypeName.Parse("Uno.UX.Selector"), nameString);
				var origin = new StringLiteral(null); // dummy origin
				
				return new CallStaticMethod(
						StaticMemberName.Parse("Uno.UX.SimulatedProperties.Set"),
						_selfExpression, nameString, value, origin) 
					+ new CallStaticMethod(
						StaticMemberName.Parse("Uno.UX.PropertyObject.EmulatePropertyChanged"),
						_selfExpression, nameSelector, origin);
			}
			
			return new WriteProperty(_selfExpression, prop.GetMemberName(), value);
		}
	}



}

