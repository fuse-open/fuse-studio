using System;
using System.IO;
using System.Linq;
using System.Xml;
using System.Xml.Linq;
using Uno.Text;
using Uno.UX.Markup;

namespace Outracks.Fuse
{
	using IO;

	sealed public class SourceFragment : IEquatable<SourceFragment>
	{
		readonly byte[] _data;
		public SourceFragment(byte[] data)
		{
			_data = data;
		}

		// byte[] <-> SourceFragment

		public static SourceFragment FromBytes(byte[] data)
		{
			return new SourceFragment(data);
		}

		public byte[] ToBytes()
		{
			return _data;
		}

		// String <-> SourceFragment
	
		public static SourceFragment FromString(string uxSourceCode)
		{
			return new SourceFragment(Utf8.GetBytes(uxSourceCode));
		}

		public override string ToString()
		{
			return Utf8.GetString(_data);
		}

		// XElement <-> SourceFragment

		public static SourceFragment FromXml(XElement xElement)
		{
			using (var file = new MemoryStream())
			using (var writer = UxWriter.Create(file))
			{
				new XDocument(xElement).Save(writer);
				writer.Flush();
				file.Seek(0, SeekOrigin.Begin);
				return new SourceFragment(file.ReadAllBytes());
			}
		}

		/// <exception cref="XmlException"></exception>
		public XElement ToXml()
		{
			using (var stream = new MemoryStream(_data))
			{
				var root = XmlHelpers.ReadAllXml(stream, LoadOptions.PreserveWhitespace | LoadOptions.SetLineInfo, uxMode: true).Root;
				root.Remove();
				return root;
			}
		}

		// Stream <-> SourceFragment

		public static SourceFragment ReadFrom(BinaryReader reader)
		{
			return new SourceFragment(reader.ReadBytes(reader.ReadInt32()));
		}
	
		public void WriteTo(BinaryWriter writer)
		{
			writer.Write(_data.Length);
			writer.Write(_data);
		}

		public static SourceFragment Empty = new SourceFragment(new byte[0]);


		// Equals 

		public override bool Equals(object obj)
		{
			if (ReferenceEquals(null, obj)) return false;
			if (ReferenceEquals(this, obj)) return true;
			return Equals((SourceFragment)obj);
		}

		public bool Equals(SourceFragment other)
		{
			if (ReferenceEquals(null, other)) return false;
			if (ReferenceEquals(this, other)) return true;
			return _data.SequenceEqual(other._data);
		}

		public override int GetHashCode()
		{
			return _data.Length;
		}

	}
}