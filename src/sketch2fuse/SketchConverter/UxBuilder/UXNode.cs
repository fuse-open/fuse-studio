using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using System.Linq;

namespace SketchImporter.UxGenerator
{
	public class UxSerializerContext
	{
		public int IndentLevel { get; set; } = 0;
		public string SingleIndent { get; set; } = "\t";

		public string Indentation
		{
			get {
				var indent = "";
				for (var i = 0; i < IndentLevel; ++i) {
					indent += SingleIndent;
				}
				return indent;
			}
		}

		public string Indent(string str)
		{
			return Indentation + str;
		}

		public UxSerializerContext WithIndent(int relativeIndentLevel = 1)
		{
			return new UxSerializerContext
			{
				IndentLevel = IndentLevel + relativeIndentLevel,
				SingleIndent = SingleIndent
			};
		}

		public UxSerializerContext WithUnindent(int relativeIndentLevel = 1) => WithIndent(-relativeIndentLevel);
	}

	public interface IUxSerializeable
	{
		string SerializeUx(UxSerializerContext ctx);
	}

	[DebuggerDisplay("{Value}")]
	public class UxString : IUxSerializeable
	{
		public string Value;
		public UxString(string value) { Value = value; }
		public string SerializeUx(UxSerializerContext ctx) => Value;
	}

	static class UxSerializationHelpers
	{
		public static string SerializeComponentFloatExact(params float[] components)
			=> string.Join(", ", components.Select(x => x.ToString(CultureInfo.InvariantCulture)));

		public static string SerializeComponentFloat(params float[] components)
		{
			var canBeFloat2 = components.Length == 4 &&
							  components[0].Equals(components[2]) &&
							  components[1].Equals(components[3]);

			if(canBeFloat2)
			{
				return SerializeComponentFloat(components[0], components[1]);
			}

			if(components.Length == 2 && components[0] == components[1])
			{
				return SerializeComponentFloatExact(components[0]);
			}

			return SerializeComponentFloatExact(components);
		}
	}

	[DebuggerDisplay("{X},{Y},{Z},{W}")]
	public class UxFloat4 : IUxSerializeable
	{
		public float X, Y, Z, W;

		public UxFloat4(float x, float y, float z, float w)
		{
			X = x;
			Y = y;
			Z = z;
			W = w;
		}

		public string SerializeUx(UxSerializerContext ctx) =>
			UxSerializationHelpers.SerializeComponentFloat(X, Y, Z, W);
	}

	[DebuggerDisplay("{X},{Y}")]
	public class UxFloat2 : IUxSerializeable
	{
		public float X, Y;

		public UxFloat2(float x, float y)
		{
			X = x;
			Y = y;
		}

		public string SerializeUx(UxSerializerContext ctx) =>
			UxSerializationHelpers.SerializeComponentFloat(X, Y);
	}

	[DebuggerDisplay("{Value}")]
	public class UxFloat : IUxSerializeable
	{
		public float Value;

		public UxFloat(float value) { Value = value; }

		public string SerializeUx(UxSerializerContext ctx) =>
			UxSerializationHelpers.SerializeComponentFloat(Value);

		public static explicit operator UxFloat(float value) => new UxFloat(value);
		public static explicit operator UxFloat(double value) => new UxFloat((float)value);
	}

	public class UxVector : IUxSerializeable, IEnumerable<IUxSerializeable>
	{
		readonly List<IUxSerializeable> _components = new List<IUxSerializeable>();
		public IEnumerable<IUxSerializeable> Components => _components;

		public UxVector(params IUxSerializeable[] components)
		{
			_components.AddRange(components);
		}

		public UxVector(params float[] components)
		{
			new UxVector(components.Select(x => new UxFloat(x)).ToArray());
		}

		public string SerializeUx(UxSerializerContext ctx)
		{
			return string.Join(", ", Components.Select(x => x.SerializeUx(ctx)));
		}

		public void Append(IUxSerializeable component)
		{
			_components.Add(component);
		}

		public IEnumerator<IUxSerializeable> GetEnumerator()
		{
			return _components.GetEnumerator();
		}

		IEnumerator IEnumerable.GetEnumerator()
		{
			return GetEnumerator();
		}
	}

	public enum UxUnit
	{
		Percent,
		Pixels,
		Points
	}

	[DebuggerDisplay("{Content}")]
	public class UxComment : IUxSerializeable
	{
		public UxComment(string content)
		{
			Content = content;
		}

		public string Content { get; set; }

		public string SerializeUx(UxSerializerContext ctx)
		{
			return ctx.Indent($"<!-- {Content} -->");
		}
	}

	[DebuggerDisplay("{X},{Y},{Z}")]
	public class UxRotation : IUxSerializeable
	{
		public enum RotationAxis
		{
			X,
			Y,
			Z
		}
		public UxRotation(double degrees, RotationAxis axis = RotationAxis.Z)
		{
			// Sketch rotates in the oposite direction, so we need the negated value. 2017-12-12 anette
			Degrees = -degrees;
			Axis = axis;
		}

		public double Degrees { get; set; }
		public RotationAxis Axis { get; set; }

		public string SerializeUx(UxSerializerContext ctx)
		{
			string attributeName;
			switch (Axis)
			{
				case RotationAxis.X:
					attributeName = "DegreesX";
					break;
				case RotationAxis.Y:
					attributeName = "DegreesY";
					break;
				default:
					attributeName = "Degrees";
					break;
			}
			return ctx.Indent($"<Rotation {attributeName}=\"{Degrees}\" />");
		}
	}

	[DebuggerDisplay("{ClassName} (From layer: '{SketchLayerName}')")]
	public class UxNode : IUxSerializeable
	{
		//TODO make stuff readonly here, and probably in the rest of the model too. Transforms can instead return new trees.
		public string ClassName { get; set; }
		public string SketchLayerName { get; set; } // TODO I don't like that we're using null to mean "no layer" here
		public string UxName { get; set; } = null;
		public Dictionary<string, IUxSerializeable> Attributes { get; set; } = new Dictionary<string, IUxSerializeable>();
		public List<IUxSerializeable> Children { get; set; } = new List<IUxSerializeable>(); //TODO would be much nicer if this was a list of UxNode, then we'd have to cast less (UxComment can inherit UxNode?)

		public UxNode() { }

		public UxNode(string className) : base()
		{
			ClassName = className;
		}

		public virtual string SerializeUx(UxSerializerContext ctx)
		{
			var str = ctx.Indent("<" + ClassName);

			if(UxName != null) {
				str += $" ux:Name=\"{UxName}\"";
			}

			if (Attributes != null && Attributes.Count > 0)
			{
				str += " " + string.Join(" ", Attributes.Select(x => x.Key + "=\"" + x.Value.SerializeUx(ctx) + '"'));
			}

			if(Children == null || Children.Count == 0) {
				return str + " />";
			}

			str += ">\n";
			str += string.Join("\n", Children.Select(child => child.SerializeUx(ctx.WithIndent())));
			str += "\n" + ctx.Indent("</" + ClassName + ">");

			return str;
		}
	}

	public class NullNode : UxNode
	{
		private readonly UxComment _comment;

		public NullNode(UxComment comment)
		{
			_comment = comment;
		}

		public override string SerializeUx(UxSerializerContext ctx)
		{
			return _comment.SerializeUx(ctx);
		}
	}
}
