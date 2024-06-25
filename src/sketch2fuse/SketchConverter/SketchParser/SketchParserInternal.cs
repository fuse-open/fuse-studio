using System;
using System.Collections.Generic;
using System.Data;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using Newtonsoft.Json.Linq;
using SketchConverter.API;
using SketchConverter.SketchModel;

namespace SketchConverter.SketchParser
{
	internal class SketchParserInternal
	{
		private readonly ISketchArchive _archive;
		private readonly ILogger _log;

		public SketchParserInternal(ISketchArchive archive, ILogger log)
		{
			_archive = archive;
			_log = log;
		}

		public async Task<SketchDocument> ParseDocumentAsync()
		{
			var documentJson = await ReadJsonFileAsync("document.json").ConfigureAwait(false);
			ExpectClass("document", documentJson);

			var pages = await Task.WhenAll(
				documentJson["pages"].Cast<JObject>().Select(ParsePageFileReferenceAsync)).ConfigureAwait(false);

			return new SketchDocument(ParseEntityId(documentJson), pages);
		}

		async Task<JObject> ReadJsonFileAsync(string path)
		{
			using (var jsonStream = _archive.OpenFile(path))
			{
				return JObject.Parse(await jsonStream.ReadToStringAsync().ConfigureAwait(false));
			}
		}

		async Task<SketchPage> ParsePageFileReferenceAsync(JObject reference)
		{
			ExpectStringAttributeEquals("_ref_class", "MSImmutablePage", reference);

			var path = ParseFileReferencePath(reference) + ".json";
			return ParsePage(await ReadJsonFileAsync(path).ConfigureAwait(false));
		}

		string ParseFileReferencePath(JObject reference)
		{
			ExpectClass("MSJSONFileReference", reference);
			return (string)reference["_ref"];
		}

		static readonly string[] ImageFileExtensions = {"png", "jpg", "jpeg"};

		SketchImage ParseImageFileReference(JObject reference)
		{
			var imageName = ParseFileReferencePath(reference);

			foreach(var ext in ImageFileExtensions)
			{
				var path = imageName + "." + ext;
				var stream = _archive.OpenFile(path);
				if(stream == null)
					continue;

				return new SketchImage(path, stream);
			}

			return null;
		}

		SketchBitmap ParseBitmap(JObject bitmapJson)
		{
			return new SketchBitmap(
				ParseCommonLayerProperties(bitmapJson),
				ParseImageFileReference((JObject) bitmapJson["image"]));
		}

		static bool GetBit(byte bits, byte index)
		{
			return ((bits >> index) & 1) == 1;
		}

		SketchAxisAlignment ParseAlignmentAxis(byte code)
		{
			return new SketchAxisAlignment(GetBit(code, 2), GetBit(code, 0), GetBit(code, 1));
		}

		SketchAlignment ParseAlignment(JObject layer)
		{
			var constraintCode = (uint)layer["resizingConstraint"];
			// For whatever reason, flags in this constant are active when the corresponding bit is zero.
			// So we flip the bits and truncate to the maximum of 6 flags
			var constraintFlags = (~constraintCode) & 0x3F;

			return new SketchAlignment(
				ParseAlignmentAxis((byte)constraintFlags),
				ParseAlignmentAxis((byte)(constraintFlags >> 3))
			);
		}

		Result<SketchLayer> ParseLayer(JObject layerJson)
		{
			var layerClass = (string)layerJson["_class"];

			try
			{
				switch (layerClass)
				{
					case "rectangle": return ParseRectangle(layerJson);
					case "shapeGroup": return ParseShapeGroup(layerJson);
					case "artboard": return ParseArtboard(layerJson);
					case "group": return ParseGroup(layerJson);
					case "bitmap": return ParseBitmap(layerJson);
					case "text": return ParseText(layerJson);

					case "symbolMaster": return ParseSymbolMaster(layerJson);
					case "symbolInstance": return ParseSymbolInstance(layerJson);

					// star is a separate class, and has properties like radius. Do we want a SketchStar-class?
					// Do we need it?
					case "star":
					case "triangle":
					case "polygon":
					case "oval"
					: // oval stores it's control points as circle and the shape is determined by the bounding box, might have to handle this differently than path. Or calculate the path coordinates
					case "shapePath": return ParseShapePath(layerJson, layerClass);
					default:
						var warning = "Skipping layer '" + (string) layerJson["name"] + "' of unsupported type '" + layerClass + "'";
						_log.Warning(warning);
						return Result.Err(warning);
				}
			}
			catch (Exception e)
			{
				var name = (string) layerJson["name"];
				var displayName = (name != null) ? "'" + name + "'" : "with unknown name";
				var error = "Failed to parse layer " + displayName + ", skipping it. The exception was :'" + e.Message + "'";
				_log.Error(error);
				return Result.Err(error);
			}
		}

		private SketchLayer ParseShapePath(JObject layerJson, string layerClass)
		{
			// We will parse multiple classes as shape path
			//ExpectClass("shapePath", layerJson);
			return new SketchShapePath(
				ParseCommonLayerProperties(layerJson),
				ParsePath((JObject)layerJson["path"]),
				ParseBooleanOperation(layerJson));
		}

		private SketchBooleanOperation ParseBooleanOperation(JObject layerJson)
		{
			var operation = (int) layerJson["booleanOperation"];
			switch (operation)
			{
				case -1:
					return SketchBooleanOperation.NoOperation;
				case 0:
					return SketchBooleanOperation.Union;
				case 1:
					return SketchBooleanOperation.Subtraction;
				case 2:
					return SketchBooleanOperation.Intersection;
				case 3:
					return SketchBooleanOperation.Difference;
				default:
					_log.Error($"Parser: {operation} is not a valid boolean operation in Sketch. Falling back to no operation");
					return SketchBooleanOperation.NoOperation;
			}
		}

		private SketchPath ParsePath(JObject path)
		{
			var isClosed = (bool) path["isClosed"];
			var pointsJson = (JArray) path["points"];
			var points = pointsJson.Cast<JObject>().Select(ParseCurvePoint);

			return new SketchPath(points.ToList(), isClosed);
		}

		private SketchCurvePoint ParseCurvePoint(JObject curvePointJson)
		{
			ExpectClass("curvePoint", curvePointJson);

			var hasCurveFrom = (bool) curvePointJson["hasCurveFrom"];
			var hasCurveTo = (bool) curvePointJson["hasCurveTo"];

			var mode = ParseCurvePointMode(curvePointJson);

			var point = ParsePoint((string) curvePointJson["point"]);
			var curveFrom = hasCurveFrom ? ParsePoint((string) curvePointJson["curveFrom"]) : point;
			var curveTo = hasCurveTo ? ParsePoint((string)curvePointJson["curveTo"]) : point;
			var cornerRadius = (double?) curvePointJson["cornerRadius"] ?? 0.0;

			return new SketchCurvePoint(
				point: point,
				curveFrom: curveFrom ,
				curveTo: curveTo,
				cornerRadius: cornerRadius,
				mode: mode,
				hasCurveFrom: hasCurveFrom,
				hasCurveTo: hasCurveTo);
		}

		private SketchCurvePoint.CurveMode ParseCurvePointMode(JObject curvePointJson)
		{
			var mode = (int)curvePointJson["curveMode"];
			switch (mode)
			{
				case 1:
					return SketchCurvePoint.CurveMode.Line;
				case 2:
					return SketchCurvePoint.CurveMode.Curve;
				case 3:
					return SketchCurvePoint.CurveMode.Asymmetric;
				case 4:
					return SketchCurvePoint.CurveMode.Disconnected;
				default:
					_log.Warning("Parse error: Unrecognized curve mode: " + mode + " fallback to Line");
					return SketchCurvePoint.CurveMode.Line;
			}
		}

		private SketchSymbolInstance ParseSymbolInstance(JObject symbolInstanceJson)
		{
			ExpectClass("symbolInstance", symbolInstanceJson);

			return new SketchSymbolInstance(
				ParseCommonLayerProperties(symbolInstanceJson),
				ParseSymbolID(symbolInstanceJson));
		}


		SketchAttributedString ParseAttributedString(JObject attributedStringJson)
		{
			ExpectClass("MSAttributedString", attributedStringJson);

			var archive = (string)((JObject) attributedStringJson["archivedAttributedString"])["_archive"];
			return new AttributedStringParser(_log).Parse(archive ?? "");
		}

		SketchText ParseText(JObject textJson)
		{
			ExpectClass("text", textJson);

			return new SketchText(
				ParseCommonLayerProperties(textJson),
				ParseAttributedString((JObject) textJson["attributedString"]),
				ParseTextBehaviour((int?)textJson["textBehaviour"] ?? 0)
			);
		}

		SketchTextBoxAlignment ParseTextBehaviour(int code)
		{
			switch(code)
			{
				case 0: return SketchTextBoxAlignment.Auto;
				case 1: return SketchTextBoxAlignment.Fixed;
				default: throw new SketchParserException($"Invalid text behaviour: {code}");
			}
		}

		SketchArtboard ParseArtboard(JObject artboardJson)
		{
			ExpectClass("artboard", artboardJson);

			return new SketchArtboard(
				ParseCommonLayerProperties(artboardJson));
		}

		public SketchSymbolMaster ParseSymbolMaster(JObject symbolMasterJson)
		{
			ExpectClass("symbolMaster", symbolMasterJson);
			return new SketchSymbolMaster(
				ParseCommonLayerProperties(symbolMasterJson),
				ParseSymbolID(symbolMasterJson));
		}

		private static Guid ParseSymbolID(JObject symbolJson)
		{
			JToken idNode;
			if (symbolJson["symbolID"] != null) idNode = symbolJson["symbolID"];
			else throw new SketchParserException("Expected symbol ID");
			var idString = (string) idNode;
			Guid id;
			if (!Guid.TryParse(idString, out id))
			{
				throw new SketchParserException("Invalid symbol ID: " + idString);
			}
			return id;
		}

		SketchRectangle ParseRectangle(JObject rectangleJson)
		{
			ExpectClass("rectangle", rectangleJson);

			var layer = ParseCommonLayerProperties(rectangleJson);
			var curvePoints = ParsePath((JObject)rectangleJson["path"]);

			return new SketchRectangle(layer, curvePoints, ParseBooleanOperation(rectangleJson));
		}

		SketchGroup ParseGroup(JObject groupJson)
		{
			ExpectClass("group", groupJson);

			return new SketchGroup(ParseCommonLayerProperties(groupJson));
		}

		SketchLayer ParseCommonLayerProperties(JObject layerJson)
		{
			var children = ParseChildLayers(layerJson).ToList();
			var parent = new SketchLayer(
				ParseEntityId(layerJson),
				null, //Parent has to be set later, since it doesn't exist yet
				ParseFrame(layerJson),
				(string) layerJson["name"],
				(bool?) layerJson["nameIsFixed"] ?? false,
				ParseAlignment(layerJson),
				(double?) layerJson["rotation"] ?? 0,
				(bool?) layerJson["isFlippedVertical"] ?? false,
				(bool?) layerJson["isFlippedHorizontal"] ?? false,
				ParseStyle(layerJson),
				children
			);

			foreach (var child in children)
			{
				child.Parent = parent; //Parent doesn't exist until children are done parsing, so set it here
			}

			return parent;
		}


		IEnumerable<SketchLayer> ParseChildLayers(JObject layer)
		{
			var layersJson = (JArray)layer["layers"];
			return layersJson == null ? new List<SketchLayer>() : (layersJson.Cast<JObject>().Select(ParseLayer).SelectOk());
		}

		SketchColor ParseColor(JObject colorJson)
		{
			ExpectClass("color", colorJson);

			return new SketchColor(
				(float?)colorJson["red"] ?? 0,
				(float?)colorJson["green"] ?? 0,
				(float?)colorJson["blue"] ?? 0,
				(float?)colorJson["alpha"] ?? 0
			);
		}

		SketchSolidColorBrush ParseSolidColorBrush(JObject brushJson)
		{
			return new SketchSolidColorBrush(ParseColor((JObject) brushJson["color"]));
		}

		SketchGradientStop ParseGradientStop(JObject gradientStopJson)
		{
			ExpectClass("gradientStop", gradientStopJson);

			return new SketchGradientStop(
				ParseColor((JObject) gradientStopJson["color"]),
				(double?) gradientStopJson["position"] ?? 0);
		}

		ISketchBrush ParseGradientBrush(JObject brushJson)
		{
			var gradientJson = (JObject)brushJson["gradient"];

			ExpectClass("gradient", gradientJson);

			var gradientType = (int?) gradientJson["gradientType"] ?? 0;
			if (gradientType != 0)
			{
				throw new SketchParserException("Only linear gradients are supported");
			}

			var stops = ((JArray) gradientJson["stops"])
				.Cast<JObject>()
				.Select(ParseGradientStop);

			return new SketchLinearGradientBrush(
				ParsePoint((string)gradientJson["from"]),
				ParsePoint((string)gradientJson["to"]),
				stops.ToList()
			);
		}

		ISketchBrush ParseFill(JObject fillJson)
		{
			var fillType = (int?) fillJson["fillType"] ?? 0;
			switch (fillType)
			{
				case 0: return ParseSolidColorBrush(fillJson);
				case 1: return ParseGradientBrush(fillJson);
				case 4: throw new SketchParserException("Image/Pattern fill is not supported");
				case 5: throw new SketchParserException("Noise fill is not supported");
				default: throw new SketchParserException("Unknown fill type '" + fillType + "'");
			}
		}

		SketchShadow ParseShadow(JObject shadowJson)
		{
			ExpectedClasses(shadowJson, "shadow", "innerShadow");

			return new SketchShadow(
				(bool?)shadowJson["isEnabled"] ?? false,
				ParseColor((JObject)shadowJson["color"]),
				new SketchPoint(
					ParseNumber((string)shadowJson["offsetX"] ?? "0"),
					ParseNumber((string)shadowJson["offsetY"] ?? "0")
				),
				ParseNumber((string)shadowJson["blurRadius"] ?? "0"),
				ParseNumber((string)shadowJson["spread"] ?? "0")
			);
		}

		SketchBorderPosition ParseBorderPosition(JObject borderJson)
		{
			if (borderJson["position"] == null)
			{
				throw new NoNullAllowedException($"Expected Sketch border to have a position");
			}

			var pos = 0;
			pos = (int) borderJson["position"];

			switch (pos)
			{
				case 0: return SketchBorderPosition.Center;
				case 1: return SketchBorderPosition.Inside;
				case 2: return SketchBorderPosition.Outside;
				default: throw new SketchParserException($"Unknown border position: {pos}");
			}
		}

		Optional<SketchStyle> ParseStyle(JObject layerJson)
		{
			var styleJson = (JObject)layerJson["style"];
			if (styleJson == null)
				return Optional.None();

			ExpectClass("style", styleJson);

			return new SketchStyle(
				ParseFills(styleJson),
				ParseBorders(styleJson),
				ParseShadows(styleJson["shadows"]),
				ParseShadows(styleJson["innerShadows"]),
				ParseBlur(styleJson),
				ParseOpacity(styleJson));
		}

		private List<SketchFill> ParseFills(JObject styleJson)
		{
			if (styleJson["fills"] == null)
				return new List<SketchFill>();

			return ((JArray) styleJson["fills"])
				.Cast<JObject>()
				.Select(fillJson => new SketchFill
				(
					(bool?) fillJson["isEnabled"] ?? false,
					ParseFill(fillJson)
				))
				.EmptyIfNull()
				.ToList();
		}

		private List<SketchBorder> ParseBorders(JObject styleJson)
		{
			if (styleJson["borders"] == null)
				return new List<SketchBorder>();

			return ((JArray) styleJson["borders"])
				.Cast<JObject>()
				.Select(borderJson => new SketchBorder
				(
					(bool?) borderJson["isEnabled"] ?? false,
					ParseFill(borderJson),
					(double?) borderJson["thickness"] ?? 0,
					ParseBorderPosition(borderJson)
				))
				.EmptyIfNull()
				.ToList();
		}

		private List<SketchShadow> ParseShadows(JToken styleJson)
		{
			if (styleJson == null)
				return new List<SketchShadow>();

			var shadowArray = styleJson as JArray;
			return shadowArray != null ? shadowArray.Cast<JObject>().Select(ParseShadow).ToList() : new List<SketchShadow>();
		}

		private Optional<SketchBlur> ParseBlur(JObject styleJson)
		{
			var blurJson = (JObject)styleJson["blur"];
			if (blurJson == null)
				return Optional.None();

			var enabled = (bool?) blurJson["isEnabled"] ?? false;
			if (!enabled)
				return Optional.None();

			return new SketchBlur(
				ParsePoint((string) blurJson["center"]),
				(int?) blurJson["motionAngle"] ?? 0,
				(double?) blurJson["radius"] ?? 0.0,
				ParseBlurType(blurJson));
		}

		private Optional<double> ParseOpacity(JObject styleJson)
		{
			var contextSettings = (JObject) styleJson["contextSettings"];
			if (contextSettings != null)
			{
				var opacity = (double?) contextSettings["opacity"];
				if (opacity != null)
				{
					return opacity.Value;
				}
			}
			return Optional.None();
		}

		private SketchBlur.SketchBlurType ParseBlurType(JObject blurJson)
		{
			var type = (int)blurJson["type"];
			switch (type)
			{
				case 0:
					return SketchBlur.SketchBlurType.Gaussian;
				case 1:
					return SketchBlur.SketchBlurType.Motion;
				case 2:
					return SketchBlur.SketchBlurType.Zoom;
				case 3:
					return SketchBlur.SketchBlurType.Background;
				default:
					_log.Warning("Parse error: Unrecognized blur type: " + type + " fallback to Gaussian");
					return SketchBlur.SketchBlurType.Gaussian;
			}
		}


		SketchShapeGroup ParseShapeGroup(JObject shapeGroupJson)
		{
			ExpectClass("shapeGroup", shapeGroupJson);
			var hasClippingMask = (bool) shapeGroupJson["hasClippingMask"];

			return new SketchShapeGroup(
				ParseCommonLayerProperties(shapeGroupJson),
				hasClippingMask);
		}

		SketchRect ParseFrame(JObject layerJson)
		{
			return ParseRect((JObject) layerJson["frame"]);
		}

		// Format of a 2D point is {double, double}, we let double.Parse in ParseNumber
		// check that the two words separated by a comma is actually valid formats of
		// double values. Extract to matches that are not whitespace, separated by `,`
		// and by some optional whitespace.
		static readonly Regex PointRegex = new Regex(@"{\s*(\S+),\s*(\S+)\s*}", RegexOptions.Compiled);

		double ParseNumber(string number)
		{
			return double.Parse(number, CultureInfo.InvariantCulture);
		}

		SketchPoint ParsePoint(string point)
		{
			var match = PointRegex.Match(point);
			if (!match.Success)
			{
				throw new SketchParserException("Invalid 2D point");
			}

			var x = ParseNumber(match.Groups[1].Value);
			var y = ParseNumber(match.Groups[2].Value);

			return new SketchPoint(x, y);
		}

		SketchRect ParseRect(JObject rectJson)
		{
			ExpectClass("rect", rectJson);
			return new SketchRect(
				(double) rectJson["x"],
				(double) rectJson["y"],
				(double) rectJson["width"],
				(double) rectJson["height"],
				(bool) rectJson["constrainProportions"]
			);
		}

		SketchPage ParsePage(JObject pageJson)
		{
			ExpectClass("page", pageJson);
			return new SketchPage(
				ParseEntityId(pageJson),
				ParseRect((JObject) pageJson["frame"]),
				(string) pageJson["name"],
				((JArray)pageJson["layers"])
				.Cast<JObject>()
				.Select(ParseLayer)
				.SelectOk()
				.ToList()
			);
		}

		Guid ParseEntityId(JObject source)
		{
			// Currently the objectID isn't used for anything so returning Guid.Empty
			// if parsing the objectID fails doesn't cause problems. But it might in
			// the future is we start using it. 2017-12-05 anette
			JToken idNode;
			if (source["do_objectID"] != null)
			{
				idNode = source["do_objectID"];
			}
			else
			{
				_log.Warning("Parser: Expected object ID, using empty Guid");
				return Guid.Empty;
			}
			var idString = (string) idNode;
			Guid id;
			if (!Guid.TryParse(idString, out id))
			{
				_log.Warning("Parser: Invalid object ID " + idString + ", using empty Guid");
				return Guid.Empty;
			}
			return id;
		}

		void ExpectStringAttributeEquals(string key, string expectedValue, JObject obj)
		{
			var actualValue = (string) obj[key];
			if (expectedValue != actualValue)
			{
				throw new InvalidAttributeException(key, expectedValue, actualValue);
			}
		}

		void ExpectClass(string expectedClass, JObject obj)
		{
			try
			{
				ExpectStringAttributeEquals("_class", expectedClass, obj);
			}

			catch (InvalidAttributeException e)
			{
				throw new SketchParserException($"Attempted to parse Sketch class '{e.ExpectedValue}', got '{e.ActualValue}'");
			}
		}

		void ExpectedClasses(JObject obj, params string[] expectedClasses)
		{
			var actualValue = (string) obj["_class"];
			if (!expectedClasses.ToList().Contains(actualValue))
			{
				throw new SketchParserException($"Attempted to parse Sketch class of either '{expectedClasses}', but was '{actualValue}'");
			}
		}
	}

	internal static class Extensions
	{
		public static IEnumerable<T> EmptyIfNull<T>(this IEnumerable<T> self)
		{
			return self ?? new T[0];
		}

		public static async Task<string> ReadToStringAsync(this Stream stream, Encoding encoding = null)
		{
			using (var text = new StreamReader(stream, encoding ?? Encoding.UTF8))
			{
				return await text.ReadToEndAsync();
			}
		}
	}
}
