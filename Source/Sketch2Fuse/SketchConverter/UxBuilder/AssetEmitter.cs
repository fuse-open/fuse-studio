using System;
using System.IO;
using SketchConverter.API;
using SketchConverter.SketchModel;

namespace SketchConverter.UxBuilder
{
	public interface IAssetEmitter
	{
		void Write(SketchImage image);
	}

	class AssetEmitter : IAssetEmitter
	{
		private readonly string _outputDirectory;
		private readonly ILogger _log;

		public AssetEmitter(string outputDirectory, ILogger log)
		{
			_outputDirectory = outputDirectory;
			_log = log;
		}

		public void Write(SketchImage image)
		{
			var path = image.Path;

			using (var stream = image.Open())
			using (var ms = new MemoryStream())
			{
				stream.CopyTo(ms);
				var data = ms.ToArray();
				Write(path, data);
			}
		}

		private void Write(string path, byte[] data)
		{
			try
			{
				var assetRelativeDir = Path.GetDirectoryName(path);
				var assetDir = Path.Combine(_outputDirectory, assetRelativeDir);
				var assetPath = Path.Combine(assetDir, Path.GetFileName(path));
				Directory.CreateDirectory(assetDir);
				File.WriteAllBytes(assetPath, data);
				_log.Info("Wrote " + path + " to " + assetPath);
			}
			catch (Exception e)
			{
				_log.Error("Writing asset failed " + e.Message);
			}
		}
	}
}
