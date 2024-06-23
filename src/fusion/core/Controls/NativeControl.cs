using System;
using System.Linq;
using System.Reflection;
using Outracks.Diagnostics;

namespace Outracks.Fusion
{
	public static class ImplementationLocator
	{
		static readonly object Mutex = new object();

		static bool _isLoaded;

		public static T CreateInstance<T>()
		{
			LoadImplementation();

			foreach (var ass in AppDomain.CurrentDomain.GetAssemblies())
			{
				try
				{
					if (ass.IsDynamic)
						continue;

					foreach (var t in ass.GetExportedTypes())
					{
						if (t.IsClass && t.GetInterface(typeof(T).FullName) != null)
						{
							return (T) Activator.CreateInstance(t);
						}
					}
				}
				catch (Exception)
				{
				}
			}

			throw new NotImplementedException(
				"Native control factory '" + typeof(T).FullName + "' not found in assemblies: " +
				AppDomain.CurrentDomain.GetAssemblies().Select(a => a.ToString()).Join(", "));
		}

		static void LoadImplementation()
		{
			lock (Mutex)
			{
				if (_isLoaded) return;

				try
				{
					switch (Platform.OperatingSystem)
					{
						case OS.Mac: Assembly.Load("Outracks.Fusion.Mac"); return;
						case OS.Windows: Assembly.Load("Outracks.Fusion.Windows"); return;
						default: throw new PlatformNotSupportedException();
					}
				}
				catch (Exception e)
				{
					throw new InitializationFailed(e);
				}
				finally
				{
					_isLoaded = true;
				}
			}
		}
	}
}