using System;
using System.Collections.Generic;
using System.Linq;
using System.Reactive;
using System.Reactive.Linq;
using System.Reactive.Subjects;
using System.Reflection;
using System.Runtime.CompilerServices;
using Outracks.IO;

namespace Outracks.Fusion.AutoReload
{
	public static class AutoReloadExtensions
	{
		public static IControl AutoReload(
			this IControl control,
			object[] parameters,
			[CallerFilePath] string filepath = "",
			[CallerMemberName] string method = "",
			Assembly callingAssembly = null)
		{
			return Fusion.AutoReload.AutoReload.Control(control, parameters, filepath, method, callingAssembly ?? Assembly.GetCallingAssembly());
		}

		public static IControl AutoReload(
			this IControl control,
			[CallerFilePath] string filepath = "",
			[CallerMemberName] string method = "")
		{
			return control.AutoReload(new object[0], filepath, method, Assembly.GetCallingAssembly());
		}
	}

	class AutoReloadMethod
	{
		public readonly AbsoluteFilePath Path;
		public readonly string MethodName;
		public readonly IObservable<AutoReloadMethod> Changed;

		public AutoReloadMethod(AbsoluteFilePath path, string methodName, IFileSystem fs)
		{
			Path = path;
			MethodName = methodName;
			Changed = fs
				.Watch(path)
				.Select(x => this)
				.Throttle(TimeSpan.FromMilliseconds(500));
		}
	}

	public static class AutoReload
	{
		static readonly Subject<string> _log = new Subject<string>();
		public static IObservable<string> Log { get { return _log; } }

		public static IControl Control(
			IControl control,
			object[] parameters,
			[CallerFilePath] string filepath = "",
			[CallerMemberName] string methodName = "",
			Assembly callingAssembly = null)
		{
			callingAssembly = callingAssembly ?? Assembly.GetCallingAssembly();
			if (string.IsNullOrEmpty(callingAssembly.Location))
				return control;

			return new AutoReloadMethod(AbsoluteFilePath.Parse(filepath), methodName, new Shell())
				.Changed
				.Select(method => Reload(method, parameters, _log.ToProgress()))
				.NotNone()
				.StartWith(control)
				.Switch();
		}

		static Optional<IControl> Reload(AutoReloadMethod method, object[] args, IProgress<string> log)
		{
			try
			{
				var referencedAssemblies = GetLoadedAssembliesPaths().Select(asm => asm.NativePath);
				var asmOpt = ControlFactory.Compile(new[] { method.Path }, log, referencedAssemblies);
				return asmOpt.SelectMany(asm => ControlFactory.CreateControl(asm, method.MethodName, args, log));
			}
			catch (Exception ex)
			{
				log.Report("Exception: " + ex + "\n");
				return Optional.None();
			}
		}

		private static IEnumerable<AbsoluteFilePath> GetLoadedAssembliesPaths()
		{
			var l = new List<AbsoluteFilePath>();
			foreach (var loadedAsm in AppDomain.CurrentDomain.GetAssemblies())
			{
				try
				{
					l.Add(loadedAsm.GetCodeBaseFilePath());
				}
				catch (Exception)
				{
				}
			}
			return l;
		}
	}
}