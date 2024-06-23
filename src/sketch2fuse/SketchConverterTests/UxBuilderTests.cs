using System;
using System.Collections.Generic;
using System.Linq;
using NSubstitute;
using NUnit.Framework;
using SketchConverter;
using SketchConverter.SketchModel;
using SketchConverter.UxBuilder;
using SketchImporter.UxGenerator;

namespace SketchConverterTests
{
	[TestFixture]
	public class UxBuilderTests
	{
		private readonly bool Mask = true; // make it more readable where masking is set

		[Test]
		public void BuildUxForSketchShapeGroupWithMaskingGivesUnsupportedWarnings()
		{
			var shapeGroup = new SketchShapeGroup(
				new SketchLayer(CreateLayer(5, 5), new List<SketchLayer>()),
				Mask);

			var parentGroup =
				new SketchGroup(
					new SketchLayer(
						new SketchLayer(
							Guid.Empty,
							new SketchLayer(
								CreateLayer(20,20),
								new List<SketchLayer>()),
							new SketchRect(0, 0, 10, 10, false),
							"Dummy",
							false,
							DummyAlignment(),
							0, false, false,
							Optional.None(),
							new List<SketchLayer>()),
						new List<SketchLayer>{shapeGroup}));
			shapeGroup.Parent = parentGroup;

			var log = new MessageListLogger();
			var uxBuilder = new UxBuilder(new SymbolClassNameBuilder(), Substitute.For<IAssetEmitter>(), log);

			var uxNode = uxBuilder.BuildLayer(parentGroup);
			Assert.That(uxNode, Is.Not.Null);

			var ux = uxNode.SerializeUx(new UxSerializerContext());
			Assert.That(ux, Is.EqualTo("<Panel Width=\"50%\" Height=\"50%\" Alignment=\"TopLeft\" Margin=\"0\">\n\t<!-- Dummy -->\n\t<!-- Masked shape group is not supported in UX -->\n</Panel>"));
			Assert.That(log.ErrorsAndWarnings().First(), Does.Match("WARNING:\tMasked shapes are not supported Dummy"));

		}

		[Test]
		public void BuildUxForSketchShapeGroupWithoutMaskingGivesNoWarnings()
		{
			var rectangle = new SketchRectangle(
				CreateLayer(5,5),
				new SketchPath(CurvePointUtils.RectanglePath(new CornerRadius(2)), true),
				SketchBooleanOperation.NoOperation);

			var parentGroup =
				new SketchGroup(
					new SketchLayer(
						new SketchLayer(
							Guid.Empty,
							new SketchLayer(
								CreateLayer(20,20),
								new List<SketchLayer>()),
							new SketchRect(0, 0, 10, 10, false),
							"Dummy",
							false,
							DummyAlignment(),
							0, false, false,
							Optional.None(),
							new List<SketchLayer>()),
						new List<SketchLayer>{rectangle}));
			rectangle.Parent = parentGroup;

			var log = new MessageListLogger();
			var uxBuilder = new UxBuilder(new SymbolClassNameBuilder(), Substitute.For<IAssetEmitter>(), log);

			var uxNode = uxBuilder.BuildLayer(parentGroup);
			Assert.That(uxNode, Is.Not.Null);

			var ux = uxNode.SerializeUx(new UxSerializerContext());

			Assert.That(ux, Is.EqualTo("<Panel Width=\"50%\" Height=\"50%\" Alignment=\"TopLeft\" Margin=\"0\">\n\t<!-- Dummy -->\n\t<Rectangle CornerRadius=\"2\" Width=\"50%\" Height=\"50%\" Alignment=\"TopLeft\" Margin=\"0\">\n\t\t<!-- Dummy -->\n\t</Rectangle>\n</Panel>"));
			Assert.That(log.ErrorsAndWarnings(), Is.Empty);

		}

		[Test]
		public void UnimplementedLayerTypeFailsGracefullyWithWarning()
		{
			var log = new MessageListLogger();
			var builder = new UxBuilder(new SymbolClassNameBuilder(), Substitute.For<IAssetEmitter>(), log);
			var uxNode = builder.BuildLayer(new UnimplementedLayerType());
			Assert.That(log.ErrorsAndWarnings().Count, Is.EqualTo(1));
			var expected = "Unimplemented layer type: UnimplementedLayerType";
			Assert.That(log.ErrorsAndWarnings()[0], Does.Match(expected));
			var ux = uxNode.SerializeUx(new UxSerializerContext());
			Assert.That(ux, Does.Match("<!-- " + expected + " -->"));
		}

		[Test]
		public void EmptyGroupReturnsNullNode()
		{
			var noLayers = new List<SketchLayer>();
			var emptyGroup = new SketchGroup(new SketchLayer(CreateLayer(10, 10), noLayers));
			var builder = new UxBuilder(new SymbolClassNameBuilder(), Substitute.For<IAssetEmitter>(), new MessageListLogger());
			var uxNode = builder.BuildLayer(emptyGroup);
			Assert.That(uxNode, Is.InstanceOf(typeof(NullNode)));
		}

		private static SketchAlignment DummyAlignment()
		{
			return new SketchAlignment(
				new SketchAxisAlignment(false, false, false),
				new SketchAxisAlignment(false, false, false));
		}

		private static SketchLayer CreateLayer(double width, double height)
		{
			return new SketchLayer(Guid.Empty, null, new SketchRect(0, 0, width, height, false),
				"Dummy", false, DummyAlignment(), 0, false, false, CreateStyle(), new List<SketchLayer>());
		}

		private static SketchStyle CreateStyle()
		{
			return new SketchStyle(new List<SketchFill>(), new List<SketchBorder>(), new List<SketchShadow>(), new List<SketchShadow>(), Optional.None(), Optional.None());
		}
	}

	class UnimplementedLayerType : SketchLayer
	{
		public UnimplementedLayerType()
			: base(new SketchLayer(
				       Guid.Empty,
				       null,
				       null,
				       "",
				       false,
				       new SketchAlignment(),
				       0,
				       false,
				       false,
				       Optional.None(),
				       new List<SketchLayer>()
			       ))
		{
		}
	}
}
