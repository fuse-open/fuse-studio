using System.IO;

namespace SketchConverter.SketchModel
{
    public class SketchImage
    {
        public string Path { get; }
        readonly byte[] _data;

        public SketchImage(string path, byte[] data)
        {
            Path = path;
            _data = data;
        }

        public SketchImage(string path, Stream stream)
        {
            Path = path;

            using (var memoryStream = new MemoryStream())
            {
                stream.CopyTo(memoryStream);
                _data = memoryStream.ToArray();
            }
        }

        public Stream Open() => new MemoryStream(_data);
    }
}