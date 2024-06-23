using System.IO;
using System.Xml;
using Outracks.IO;
using Uno.UX.Markup;

namespace Outracks.Fuse
{
	public class UxWriter : XmlWriter
	{
		readonly MemoryStream _buffer = new MemoryStream();
		readonly XmlWriter _bufferWriter;
		readonly TextWriter _destinationWriter;

		bool _isDisposed;

		new public static UxWriter Create(Stream stream)
		{
			return new UxWriter(stream);
		}

		UxWriter(Stream destination)
		{
			_bufferWriter = Create(
				_buffer,
				new XmlWriterSettings
				{
					OmitXmlDeclaration = true,
					Indent = false,
				});
			_destinationWriter = new StreamWriter(destination);
		}

		public override void WriteStartDocument()
		{
			WriteStartDocument(false);
		}

		public override void WriteStartDocument(bool standalone)
		{
		}

		public override void WriteEndDocument()
		{
			_bufferWriter.WriteEndDocument();
		}

		public override void WriteDocType(string name, string pubid, string sysid, string subset)
		{
		}

		public override void WriteStartElement(string prefix, string localName, string ns)
		{
			if (ns == Configuration.UXNamespace)
			{
				prefix = "ux";
			}

			_bufferWriter.WriteStartElement(prefix, localName, string.IsNullOrEmpty(prefix) ? "" : ns);
		}

		public override void WriteEndElement()
		{
			_bufferWriter.WriteEndElement();
		}

		public override void WriteFullEndElement()
		{
			_bufferWriter.WriteFullEndElement();
		}

		public override void WriteStartAttribute(string prefix, string localName, string ns)
		{
			if (ns == Configuration.UXNamespace)
			{
				prefix = "ux";
			}

			_bufferWriter.WriteStartAttribute(prefix, localName, ns);
		}

		public override void WriteEndAttribute()
		{
			_bufferWriter.WriteEndAttribute();
		}

		public override void WriteCData(string text)
		{
			_bufferWriter.WriteRaw(text);
		}

		public override void WriteComment(string text)
		{
			_bufferWriter.WriteComment(text);
		}

		public override void WriteProcessingInstruction(string name, string text)
		{
			_bufferWriter.WriteProcessingInstruction(name, text);
		}

		public override void WriteEntityRef(string name)
		{
			_bufferWriter.WriteEntityRef(name);
		}

		public override void WriteCharEntity(char ch)
		{
			_bufferWriter.WriteCharEntity(ch);
		}

		public override void WriteWhitespace(string ws)
		{
			_bufferWriter.WriteWhitespace(ws);
		}

		public override void WriteString(string text)
		{
			_bufferWriter.WriteString(text);
		}

		public override void WriteSurrogateCharEntity(char lowChar, char highChar)
		{
			_bufferWriter.WriteSurrogateCharEntity(lowChar, highChar);
		}

		public override void WriteChars(char[] buffer, int index, int count)
		{
			_bufferWriter.WriteChars(buffer, index, count);
		}

		public override void WriteRaw(char[] buffer, int index, int count)
		{
			_bufferWriter.WriteRaw(buffer, index, count);
		}

		public override void WriteRaw(string data)
		{
			_bufferWriter.WriteRaw(data);
		}

		public override void WriteBase64(byte[] buffer, int index, int count)
		{
			_bufferWriter.WriteBase64(buffer, index, count);
		}

		public override void Flush()
		{
			_bufferWriter.Flush();
			_buffer.Seek(0, SeekOrigin.Begin);
			_destinationWriter.Write(_buffer.ReadToEnd().Replace(" xmlns:ux=\"http://schemas.fusetools.com/ux\"", ""));
			_destinationWriter.Flush();
			_buffer.Seek(0, SeekOrigin.Begin);
		}

		public override string LookupPrefix(string ns)
		{
			return null;
		}

		protected override void Dispose(bool disposing)
		{
			if (_isDisposed) return;
			Flush();
			_destinationWriter.Dispose();
			_bufferWriter.Dispose();
			_buffer.Dispose();
			_isDisposed = true;
		}

		public override WriteState WriteState
		{
			get { return _bufferWriter.WriteState; }
		}
	}
}