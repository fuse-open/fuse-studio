using Uno.IO;
using Uno.Collections;
using Outracks.Simulator.Bytecode;

namespace Fuse.Simulator
{
	class BundleManager
	{
		readonly List<BundleFile> _defaultFiles;

		public BundleManager()
		{
			_defaultFiles = new List<BundleFile>();
			_defaultFiles.AddRange(GetBundle().Files);
			ResetBundle(GetBundle().ToString());
		}

		public Bundle GetBundle()
		{
			return Bundle.Get("FusePreview");
		}

		public void ResetBundle(string bundleName)
		{
			Outracks.Simulator.Runtime.Bundle.Initialize(bundleName);
			// TODO: We need to be able to delete stuff from a bundle...
		}

		public void CreateCopyOfBundleTo(Bundle bundle, string relativePath, BytecodeCache bc)
		{
			// TODO: Zip files instead of just saving them.
			var dataDir = GetDataDir();
			var bundlePath = Path.Combine(dataDir, relativePath, "bundle");

			var byteCodeDir = Path.Combine(dataDir, relativePath, "bytecode");
			CreateDirectoryRecursive(byteCodeDir);
			using(var writer = new System.IO.BinaryWriter(File.Open(Path.Combine(byteCodeDir, "bc"), FileMode.Create)))
				bc.WriteDataTo(writer);

			var files = bundle.Files.Except(_defaultFiles);
			foreach(var file in files)
			{
				var basePath = Path.Combine(bundlePath, file.DirectoryName);
				if(file.DirectoryName != null)
					CreateDirectoryRecursive(basePath);

				debug_log("Creating file: " + Path.Combine(basePath, file.Name));
				File.WriteAllBytes(Path.Combine(basePath, file.Name), file.ReadAllBytes());
			}
		}

		public BytecodeCache UpdateBundleFrom(Bundle bundle, string relativePath)
		{
			ResetBundle(bundle.ToString());
			var dataDir = GetDataDir();
			var bundlePath = Path.Combine(dataDir, relativePath, "bundle");
			UpdateBundleFromRecursive(bundle, bundlePath, "");

			var byteCodeDir = Path.Combine(dataDir, relativePath, "bytecode");
			using(var reader = new System.IO.BinaryReader(File.OpenRead(Path.Combine(byteCodeDir, "bc"))))
				return BytecodeCache.ReadDataFrom(reader);
		}

		public bool HasBundle(string relativePath)
		{
			var dataDir = GetDataDir();
			return Directory.Exists(Path.Combine(dataDir, relativePath));
		}

		void UpdateBundleFromRecursive(Bundle bundle, string path, string relPath)
		{
			foreach(var file in Directory.EnumerateFiles(path)) 
			{
				var sourcePath = Path.Combine(relPath, Path.GetFileName(file)).ToUnixPath();
				debug_log("Loading " + sourcePath);
				Outracks.Simulator.Runtime.Bundle.AddOrUpdateFile(sourcePath, File.ReadAllBytes(file));
			}

			foreach(var dir in Directory.EnumerateDirectories(path)) 
			{
				var dirName = dir.Split(new char[] { Path.DirectorySeparatorChar }).Last();
				UpdateBundleFromRecursive(bundle, Path.Combine(path, dirName), Path.Combine(relPath, dirName));
			}
		}

		void CreateDirectoryRecursive(string path)
		{
			if(Directory.Exists(path))
				return;
			
			CreateDirectoryRecursive(Path.GetDirectoryName(path));
			Directory.CreateDirectory(path);
		}

		public static string GetDataDir()
		{
			var dataDir = Directory.GetUserDirectory(UserDirectory.Data);
			if defined(DOTNET)
			{
				dataDir = Path.Combine(dataDir, ".fuse");
			}
			return dataDir;
		}
	}

	static class BundleManagerExtensions
	{
		public static IEnumerable<T> Except<T>(this IEnumerable<T> first, IEnumerable<T> second)
		{
			List<T> result = new List<T>();
			foreach(var source in first)
			{
				if(!second.Contains(source))
					result.Add(source);
			}
			return result;
		}

		public static string ToUnixPath(this string str)
		{
			return str.Replace('\\', '/');
		}
	}
}