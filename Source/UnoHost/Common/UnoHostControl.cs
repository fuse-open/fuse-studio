using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Reactive.Subjects;
using System.Reflection;
using Outracks.Diagnostics;

namespace Outracks.Fusion
{
	using IO;

	public static class UnoHostControl
	{
		static IUnoHostControlFactory _factory;
		public static IUnoHostControl Create(AbsoluteFilePath assemblyPath, Command onFocused, Menu contextMenu, AbsoluteFilePath userDataPath, Action<IUnoHostControl> initialize, Action<OpenGlVersion> gotVersion, params string[] arguments)
		{
			return GetImplementation().Create(assemblyPath, onFocused, contextMenu, userDataPath, initialize, gotVersion, arguments);
		}

		static IUnoHostControlFactory GetImplementation()
		{
			if (_factory != null)
				return _factory;

			if (Platform.OperatingSystem == OS.Mac)
				Assembly.Load("UnoHost");
			if (Platform.OperatingSystem == OS.Windows)
				Assembly.Load("Outracks.UnoHost.Windows");

			return _factory = ImplementationLocator.CreateInstance<IUnoHostControlFactory>();
		}
	}


	public interface IUnoHostControlFactory
	{
		IUnoHostControl Create(
			AbsoluteFilePath assemblyPath,
			Command onFocused,
			Menu contextMenu,
			AbsoluteFilePath userDataPath,
			Action<IUnoHostControl> initialize,
			Action<OpenGlVersion> gotVersion,
			params string[] arguments);
	}

	public interface IUnoHostControl : IDisposable
	{
		IControl Control { get; }
		IConnectableObservable<IBinaryMessage> Messages { get; }
		IObserver<IBinaryMessage> MessagesTo { get; }
		IObservable<Process> Process { get; }
	}

	// Temporary duplicate of message declared in Simulator.Common (dependency from fusion to unohost needs to be reversed)
	public sealed class Ready : IBinaryMessage
	{
		public static readonly string MessageType = "Ready";

		public string Type { get { return MessageType; } }

		public void WriteDataTo(BinaryWriter writer)
		{
		}

		public static Ready ReadDataFrom(BinaryReader reader)
		{
			return new Ready();
		}
	}
}