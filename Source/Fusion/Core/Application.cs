using System;
using System.Collections.Generic;
using System.Reactive.Concurrency;
using System.Reflection;
using System.Threading.Tasks;
using Outracks.Diagnostics;
using Outracks.IO;

namespace Outracks.Fusion
{
	public class InitializationFailed : Exception
	{
		public InitializationFailed(Exception innerException)
			: base("Failed to initialize Native UI layer: " + innerException.Message, innerException)
		{ }
	}

	public static class Application
	{
		public static event Action Terminating
		{
			add { _implementation.Terminating += value; }
			remove { _implementation.Terminating -= value; }
		}

		public static Func<IDocument, Window> CreateDocumentWindow
		{
			get; set;
		}

		public static Action LaunchedWithoutDocuments
		{
			get; set;
		}

		public static IObservable<Window> DocumentOpened
		{
			get { return _implementation.DocumentOpened; }
		}
 
		static Application()
		{
			CreateDocumentWindow = doc =>
			{
				throw new InvalidOperationException("Application.CreateDocumentWindow should be set before calling Application.Run()");
			};

			LaunchedWithoutDocuments = () =>
			{
				throw new InvalidOperationException("Application.LaunchedWithoutDocuments should be set before calling Application.Run()");
			};
		}

		static IApplication _implementation;
		
		public static IApplication Initialize(IList<string> args)
		{
			_implementation = ImplementationLocator.CreateInstance<IApplication>();
			_implementation.Initialize(args);
			return _implementation;
		}

		public static bool InitializeAsDocumentApp(IList<string> args, string applicationName)
		{
			_implementation = ImplementationLocator.CreateInstance<IApplication>();
			return _implementation.InitializeDocumentApp(args, applicationName);
		}

		public static void Run()
		{
			GetImplementationOrThrow().Run();
		}

		public static void Run(Window window)
		{
			GetImplementationOrThrow().Run(window);
		}

		public static void Exit(byte exitCode)
		{
			GetImplementationOrThrow().Exit(exitCode);
		}

		public static IDialog<object> Desktop
		{
			get { return GetImplementationOrThrow().Desktop; }
		}

		public static void ShowOpenDocumentDialog(params FileFilter[] documentTypes)
		{
			GetImplementationOrThrow().ShowOpenDialog(documentTypes);
		}

		public static Task<IDocument> OpenDocument(AbsoluteFilePath path, bool showWindow = false)
		{
			return GetImplementationOrThrow().OpenDocument(path, showWindow);
		}

		public static Menu EditMenu
		{
			get { return GetImplementationOrThrow().EditMenu; }
		}

		public static IObservable<long> PerFrame { get; set; }

		public static IScheduler MainThread { get; set; }

		public static Task<T> InvokeAsync<T>(this IScheduler dispatcher, Func<T> func)
		{
			var tcs = new TaskCompletionSource<T>();

			dispatcher.Schedule(() =>
			{
				try
				{
					var result = func();
					tcs.SetResult(result);
				}
				catch (Exception e)
				{
					tcs.SetException(e);
				}
			});

			return tcs.Task;
		}

		static IApplication GetImplementationOrThrow()
		{
			return _implementation.ThrowIfNull(new InvalidOperationException());
		}
	}

	public interface IApplication
	{
		// TODO: replace with static initialized NotificationIcon 
		ITrayApplication CreateTrayApplication(IReport errorHandler, IObservable<string> title, Menu menu, IObservable<Icon> icon);

		void Initialize(IList<string> args);
		bool InitializeDocumentApp(IList<string> args, string applicationName);
		void Run();
		void Run(Window window);
		void Exit(byte exitCode);

		void ShowOpenDialog(params FileFilter[] documentTypes);
		Task<IDocument> OpenDocument(AbsoluteFilePath path, bool giveFocus = false);
		IObservable<Window> DocumentOpened { get; }

		IDialog<object> Desktop { get; }
		Menu EditMenu { get; }

		event Action Terminating;
	}


}