using System;
using System.Globalization;
using System.Linq;
using System.Text;
using Claunia.PropertyList;
using SketchConverter.API;
using SketchConverter.SketchModel;

namespace SketchConverter.SketchParser
{
	public class AttributedStringParser
	{
		public AttributedStringParser(ILogger log)
		{
			_log = log;
		}

		NSKeyedArchive _archive;
		private readonly ILogger _log;

		public SketchAttributedString Parse(string base64Plist)
		{
			var binaryPlist = Convert.FromBase64String(base64Plist);
			var archiveRoot = PropertyListParser.Parse(binaryPlist) as NSDictionary;
			_archive = new NSKeyedArchive(archiveRoot);

			var root = _archive.Parse() as NSDictionary;

			// We can have more than one attribute for a text object in Sketch
			var attributes = root.Get("NSAttributes") ?? new NSArray(0);

			var attributeDicts = attributes as NSArray;
			if (attributeDicts == null)
			{
				var singleAttribute = attributes as NSDictionary;

				attributeDicts =
					(singleAttribute == null)
					? new NSArray(0)
					: new NSArray(singleAttribute);
			}

			var content = root.GetAs<NSString>("NSString").Content;
			var sketchStringAttributes = attributeDicts
				.Cast<NSDictionary>()
				.Select(ParseAttribute)
				.ToList();
			return new SketchAttributedString
			(
				content,
				sketchStringAttributes
			);
		}

		SketchStringAttribute ParseAttribute(NSDictionary attributeDict)
		{
			var fontDescriptorAttributes = attributeDict
				.GetAs<NSDictionary>("MSAttributedStringFontAttribute")
				.GetAs<NSDictionary>("NSFontDescriptorAttributes");

			var alignment = SketchTextAlignment.Left;

			if (attributeDict.ContainsKey("NSParagraphStyle"))
			{
				var paragraphStyle = attributeDict.GetAs<NSDictionary>("NSParagraphStyle");
				if (paragraphStyle.ContainsKey("NSAlignment"))
				{
					alignment = ParseAlignment(paragraphStyle.GetAs<NSNumber>("NSAlignment"));
				}
			}

			// Looks like the color format of text objects in Sketch has changed between version 91 and 93
			// I've only found MSAttributedStringColorDictionaryAttribute in version 93. 2018-01-05 anette
			var sketchColor = SketchColor.Black;
			if (attributeDict.ContainsKey("NSColor"))
			{
				var nsDictionaryColor = attributeDict.GetAs<NSDictionary>("NSColor");
				sketchColor = ParseColor(nsDictionaryColor);
			}
			else if (attributeDict.ContainsKey("MSAttributedStringColorDictionaryAttribute"))
			{
				var components = attributeDict.GetAs<NSDictionary>("MSAttributedStringColorDictionaryAttribute");
				sketchColor = new SketchColor(
					double.Parse(components["red"].ToString()),
					double.Parse(components["green"].ToString()),
					double.Parse(components["blue"].ToString()),
					components.ContainsKey("alpha") ? double.Parse(components["alpha"].ToString()) : 1.0);
			}
			else
			{
				_log.Warning("Could not find color property for text object. Default to black.");
			}
			var fontSize = (double) fontDescriptorAttributes.GetAs<NSNumber>("NSFontSizeAttribute");
			return new SketchStringAttribute
			(
				sketchColor,
				fontSize,
				alignment
			);
		}

		SketchTextAlignment ParseAlignment(NSNumber code)
		{
			// Note: These are only correct on macOS, because of course they use slightly different values on iOS...
			switch ((int) code)
			{
				case 0: return SketchTextAlignment.Left;
				case 1: return SketchTextAlignment.Right;
				case 2: return SketchTextAlignment.Center;
				case 3: return SketchTextAlignment.Justified;
				case 4: return SketchTextAlignment.Natural;
				default: throw new SketchParserException("Invalid text alignment");
			}
		}

		SketchColor ParseColor(NSDictionary color)
		{
			var rgbStringData = color["NSRGB"] as NSData;
			var rgbString = Encoding.ASCII.GetString(rgbStringData.Bytes).TrimEnd('\0');

			var components = rgbString
				.Split(' ', '\t')
				.Where(x => !string.IsNullOrWhiteSpace(x))
				.Select(ParseNumber)
				.ToArray();

			return new SketchColor
			(
				components[0],
				components[1],
				components[2],
				(components.Length == 4)
					? components[3]
					: 1.0
			);
		}

		static double ParseNumber(string number) =>
			double.Parse(number, CultureInfo.InvariantCulture);
	}

	public class NSKeyedArchiveException : Exception
	{
		public NSKeyedArchiveException(string message) : base(message)
		{

		}
	}

	public class NSKeyedArchive
	{
		NSDictionary _archive;

		NSArray _objects;
		NSArray Objects => _objects ?? (_objects = _archive.GetAs<NSArray>("$objects"));

		public NSObject RootObject => ResolveObject(_archive.GetAs<NSDictionary>("$top")["root"]);

		public NSKeyedArchive(NSDictionary archive)
		{
			_archive = archive;

			var archiverName = (string) _archive.GetAs<NSString>("$archiver");
			if (archiverName != "NSKeyedArchiver")
			{
				throw new ArgumentException("Invalid NSKeyedArchiver dictionary", nameof(archive));
			}

			var version = _archive.GetAs<NSNumber>("$version");
			if (version.ToInt() != 100000)
			{
				throw new ArgumentException("Unsupported NSKeyedArchiver version", nameof(archive));
			}
		}

		public NSObject Parse() => ResolveObject(RootObject);

		NSObject ResolveObject(NSObject obj)
		{
			var uid = obj as UID;
			if (uid != null)
			{
				return ResolveObject(ObjectById(uid.ToInt32()));
			}

			var dict = obj as NSDictionary;
			if (dict != null)
			{
				foreach (var key in dict.Keys.ToArray())
				{
					dict[key] = ResolveObject(dict[key]);
				}

				if (dict.ContainsKey("CF$UID"))
				{
					return ResolveObjectReference(dict);
				}

				var classInfo = dict.Get("$class") as NSDictionary;
				if (classInfo != null && classInfo.ContainsKey("$classes"))
				{
					return ResolveObjectWithClassInfo(dict, classInfo);
				}
			}

			var array = obj as NSArray;
			if (array != null)
			{
				for (var i = 0; i < array.Count; i++)
				{
					array[i] = ResolveObject(array[i]);
				}
			}

			return obj;
		}

		public NSObject ResolveObjectWithClassInfo(NSDictionary dict, NSDictionary classInfo)
		{
			var classes = classInfo.GetAs<NSArray>("$classes");

			if (classes.Contains("NSDictionary")) return ParseArchivedDictionary(dict);
			if (classes.Contains("NSArray")) return ParseArchivedArray(dict);
			if (classes.Contains("NSString")) return ParseArchivedString(dict);

			return dict;
		}

		public NSObject ResolveObjectReference(NSDictionary reference)
		{
			return ResolveObject(ObjectById(ParseObjectReferenceId(reference)));
		}

		public int ParseObjectReferenceId(NSDictionary reference)
		{
			return (int)reference.GetAs<NSNumber>("CF$UID");
		}

		public NSDictionary ParseClassInfo(NSDictionary @object)
		{
			return ResolveObject(@object["$class"]) as NSDictionary;
		}

		public NSArray ParseArchivedArray(NSDictionary archivedArray)
		{
			var objects = archivedArray.GetAs<NSArray>("NS.objects");

			var resolvedArray = new NSArray();

			foreach(var @object in objects)
			{
				resolvedArray.Add(ResolveObject(@object));
			}

			return resolvedArray;
		}

		public NSString ParseArchivedString(NSDictionary archivedString)
		{
			return archivedString.GetAs<NSString>("NS.string");
		}

		public NSDictionary ParseArchivedDictionary(NSDictionary archivedDictionary)
		{
			var keys = archivedDictionary.GetAs<NSArray>("NS.keys");
			var objects = archivedDictionary.GetAs<NSArray>("NS.objects");

			if (keys == null || objects == null)
				throw new NSKeyedArchiveException("Invalid archived NSDictionary");

			if(keys.Count != objects.Count)
				throw new NSKeyedArchiveException("NSDictionary: Different number of keys and values");

			var dict = new NSDictionary();

			for(var i = 0; i < keys.Count; ++i)
			{
				var stringKey = ResolveObject(keys[i]) as NSString;
				if (stringKey == null)
				{
					throw new NSKeyedArchiveException("Non-string keys in archived NSDictionaries are not supported");
				}

				var value = ResolveObject(objects[i]);

				dict.Add((string)stringKey, value);
			}

			return dict;
		}

		T ObjectByIdAs<T>(int id) where T : NSObject => Objects.GetAs<T>(id);

		NSObject ObjectById(int id) => ObjectByIdAs<NSObject>(id);
	}

	static class NSArrayExtensions
	{
		public static T GetAs<T>(this NSArray array, int index)
			where T : NSObject
		{
			return array[index] as T;
		}
	}

	static class NSDictionaryExtensions
	{
		public static T GetAs<T>(this NSDictionary dict, string key)
			where T : NSObject
		{
			return dict[key] as T;
		}
	}

	static class UIDExtensions
	{
		public static int ToInt32(this UID uid)
		{
			if (uid == null)
				return 0;

			if (uid.Bytes.Length == 1)
			{
				return uid.Bytes[0];
			}

			var bytes = uid.Bytes.Reverse().ToArray();

			return bytes.TryConvert(x => (int)BitConverter.ToInt16(x, 0))
				   ?? bytes.TryConvert(x => BitConverter.ToInt32(x, 0))
				   ?? 0;
		}

		static T? TryConvert<T>(this byte[] bytes, Func<byte[], T> convertFn)
			where T : struct
		{
			try
			{
				return convertFn(bytes);
			}
			catch
			{
				return null;
			}
		}
	}
}
