using System;
using System.Collections.Immutable;
using System.IO;
using System.Reactive.Concurrency;
using System.Reactive.Linq;
using System.Reactive.Threading.Tasks;
using System.Reflection;
using Fuse.Preview;
using Outracks.Fusion;
using Outracks.IO;
using Outracks.Simulator.Client;
using Outracks.Simulator.Runtime;

namespace Outracks.UnoHost
{
	public sealed class LoadPlugin : IBinaryMessage
	{
		public static readonly string MessageType = "UnoHost.LoadPlugin";
		public string Type { get { return MessageType; } }

		public static LoadPlugin FromType<T>()
		{
			var type = typeof (T);
			return new LoadPlugin
			{
				_assemblyName = type.Assembly.GetCodeBaseFilePath().NativePath,
				_typeName = type.FullName,
			};
		}

		public Type LoadType()
		{
			return Assembly.LoadFrom(_assemblyName).GetType(_typeName);
		}

		string _assemblyName;
		string _typeName;

		public override bool Equals(object obj)
		{
			var that = obj as LoadPlugin;
			return that != null
				&& that._assemblyName == _assemblyName
				&& that._typeName == _typeName;
		}

		public override int GetHashCode()
		{
			return 0;
		}

		public void WriteDataTo(BinaryWriter writer)
		{
			writer.Write(_assemblyName);
			writer.Write(_typeName);
		}

		public static LoadPlugin ReadDataFrom(BinaryReader reader)
		{
			return new LoadPlugin
			{
				_assemblyName = reader.ReadString(),
				_typeName = reader.ReadString(),
			};
		}
	}

	public interface IPluginFactory
	{
		Plugin Create(PluginContext context);
	}

	public class PluginContext
	{
		public IObservable<IBinaryMessage> Input { get; set; }
		public IObservable<double> PerFrame { get; set; }
		public IScheduler Dispatcher { get; set; }

		public dynamic Reflection
		{
			get { return (Uno.Application.Current as dynamic).Reflection; }
		}
	}

	public class Plugin
	{
		public IObservable<IBinaryMessage> Output { get; set; }
		public IControl Overlay { get; set; }
	}

	public static class PluginLoader
	{
		public static IObservable<IBinaryMessage> LoadPlugin<T>(this IUnoHostControl control, IObservable<IBinaryMessage> input)
		{
			return Observable
				.Create<IBinaryMessage>(async observer =>
				{
					var outputSub = control.Messages.Subscribe(observer.OnNext);

					var ping = UnoHost.LoadPlugin.FromType<T>();
					var gotPong = control.Messages.TryParse(UnoHost.LoadPlugin.MessageType, UnoHost.LoadPlugin.ReadDataFrom)
						.Where(pong => ping.Equals(pong))
						.FirstAsync().ToTask();

					control.MessagesTo.OnNext(ping);
					
					await gotPong;

					var inputSub = input.Subscribe(control.MessagesTo.OnNext);

					return Disposable.Combine(outputSub, inputSub);
				})
				.Publish().RefCount();
		}
	}

	public class PluginManager
	{
		public static object Initialize(
			IObservable<IBinaryMessage> messagesFrom,
			IObserver<IBinaryMessage> messagesTo,
			IScheduler dispatcher,
			IObservable<double> perFrame,
			Size<IObservable<Points>> size)
		{
			var pluginManager = new PluginManager(
				new PluginContext
				{
					Input = messagesFrom,
					PerFrame = perFrame,
					Dispatcher = dispatcher
				});

			pluginManager.Overlay.Mount(
				new MountLocation.Mutable
				{
					AvailableSize = size,
					NativeFrame = ObservableMath.RectangleWithSize(size),
					IsRooted = Observable.Return(true),
				});

			pluginManager.Output.Subscribe(messagesTo);

			return pluginManager.Overlay.NativeHandle;
		}

		public PluginManager(PluginContext context)
		{
			var plugins = context.Input
				.TryParse(LoadPlugin.MessageType, LoadPlugin.ReadDataFrom)
				.Select(loadMessage =>
				{
					try
					{
						Console.WriteLine("Loading plugin from " + loadMessage);
						var pluginFactory = (IPluginFactory)Activator.CreateInstance(loadMessage.LoadType());
						var plugin = pluginFactory.Create(context);
						Console.WriteLine("Success!");

						// send message back so we can wait for it before we start pushing input
						plugin.Output = plugin.Output.StartWith(loadMessage);

						return plugin;
					}
					catch (Exception e)
					{
						Console.WriteLine("Failed: " + e);
						throw;
					}
				})
				.Retry()
				.Scan(ImmutableList<Plugin>.Empty, (list, plugin) => list.Add(plugin))
				.Replay(1);

			plugins.Connect();

			Output = plugins
				.SelectPerElement(p => p.Output)
				.Select(outputs => outputs.Merge())
				.Switch();

			Overlay = plugins
				.SelectPerElement(p => p.Overlay)
				.Layer();
		}

		public IObservable<IBinaryMessage> Output { get; set; }
		public IControl Overlay { get; set; }
	}
}
