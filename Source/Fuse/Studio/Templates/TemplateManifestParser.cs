using System.IO;
using System.Xml.Serialization;

namespace Outracks.Fuse.Templates
{
	[XmlRoot("TemplateManifest")]
	public class TemplateManifest
	{
		[XmlAttribute("Name")] public string Name;

		[XmlElement("Description")] public string Description;

		[XmlElement("Icon")] public string Icon;
	
		[XmlAttribute("Priority")] public string Priority;

		[XmlElement("FileExt")] public string FileExt;

		[XmlElement("Alias")] public string Alias;

		public static TemplateManifest Deserialize(Stream stream)
		{
			var ret = (TemplateManifest)Serializer.Deserialize(stream);
			return ret;
		}

		static XmlSerializer _serializer;

		static XmlSerializer Serializer
		{
			get
			{
				return _serializer ?? (_serializer = new XmlSerializer(typeof (TemplateManifest)));
			}
		}
	}

	public static class TemplateManifestParser
	{
		public static TemplateManifest Parse(Stream stream)
		{
			return TemplateManifest.Deserialize(stream);
		}
	}
}