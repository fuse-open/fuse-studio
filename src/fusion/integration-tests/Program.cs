// ***********************************************************************
// Copyright (c) 2015 Charlie Poole
//
// Permission is hereby granted, free of charge, to any person obtaining
// a copy of this software and associated documentation files (the
// "Software"), to deal in the Software without restriction, including
// without limitation the rights to use, copy, modify, merge, publish,
// distribute, sublicense, and/or sell copies of the Software, and to
// permit persons to whom the Software is furnished to do so, subject to
// the following conditions:
//
// The above copyright notice and this permission notice shall be
// included in all copies or substantial portions of the Software.
//
// THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND,
// EXPRESS OR IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF
// MERCHANTABILITY, FITNESS FOR A PARTICULAR PURPOSE AND
// NONINFRINGEMENT. IN NO EVENT SHALL THE AUTHORS OR COPYRIGHT HOLDERS BE
// LIABLE FOR ANY CLAIM, DAMAGES OR OTHER LIABILITY, WHETHER IN AN ACTION
// OF CONTRACT, TORT OR OTHERWISE, ARISING FROM, OUT OF OR IN CONNECTION
// WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE SOFTWARE.
// ***********************************************************************

using System;
using System.IO;
using System.Reactive.Linq;
using System.Reactive.Subjects;
using System.Text;
using System.Threading;
using NUnit.Common;
using Outracks.Extensions;
using Outracks.Fusion;
using Outracks.Fusion.IntegrationTests;

namespace NUnitLite.Tests
{
	public class Program
	{
		class ConsoleAndDialogLogWriter : TextWriter
		{
			readonly Subject<string> _chunks = new Subject<string>();
			readonly TextWriter _consoleOut;
			readonly object _lock = new object();

			public ConsoleAndDialogLogWriter(TextWriter consoleOut)
			{
				_consoleOut = consoleOut;
			}

			public override Encoding Encoding
			{
				get { return Encoding.UTF8; }
			}

			public IObservable<string> Chunks
			{
				get { return _chunks; }
			}

			public override void Write(char[] buffer, int index, int count)
			{
				_consoleOut.Write(buffer, index, count);
				lock (_lock)
					_chunks.OnNext(new string(buffer, index, count));
			}

			public override void Write(char value)
			{
				_consoleOut.Write(value);
				lock (_lock)
					_chunks.OnNext(value.ToString());
			}
		}

		/// <summary>
		/// The main program executes the tests. Output may be routed to
		/// various locations, depending on the arguments passed.
		/// </summary>
		/// <remarks>Run with --help for a full list of arguments supported</remarks>
		/// <param name="args"></param>
		[STAThread]
		public static int Main(string[] args)
		{
			Thread.CurrentThread.SetInvariantCulture();

			ControlLeakTests._application = Application.Initialize(args);

			var log = new ReplaySubject<string>();
			var writer = new ConsoleAndDialogLogWriter(Console.Out);
			writer.Chunks.Subscribe(log);
			Console.SetOut(writer);

			int returnValue = -1;

			Application.Desktop.CreateSingletonWindow(
				isVisible: Observable.Return(true),
				window: window =>
				{
					ThreadPool.QueueUserWorkItem(
						_ =>
						{
							Thread.Sleep(300);
							returnValue = new AutoRun().Execute(args, new ExtendedTextWrapper(writer), Console.In);
							Application.Exit((byte)returnValue);
						});
					return new Window
					{
						Title = Observable.Return("Integration tests"),
						//Menu = Menu.Create(MenuItem.Create("Debug", createDebugWindow)),
						Content = LogView.Create(log.Select(s => s), Observable.Return(Color.Black))
							.WithHeight(200)
							.WithBackground(Color.White)

					};
				});

			Application.Run();
			return returnValue;

		}
	}
}