using System;
using System.IO;
using System.IO.Pipes;
using System.Runtime.InteropServices;
using System.Threading;
using System.Threading.Tasks;

namespace Outracks.IPC
{
	public class NamedPipes : IPipeImpl
	{
		public static bool AreSupported
		{
			get { return Environment.OSVersion.Platform == PlatformID.Win32NT; }
		}

		public Task<Stream> Connect(PipeName name)
		{
			return Task.Run(() =>
			{
				var pipeName = name.ToString();
				var stream = new NamedPipeClientStream(pipeName);

				while (NamedPipeDoesNotExist(pipeName))
				{
					Thread.Sleep(10);
				}

				stream.Connect();
				return (Stream)stream;
			});
		}

		public Task<Stream> Host(PipeName name)
		{
			return Task.Run(() =>
			{
				var stream = new NamedPipeServerStream(name.ToString());
				stream.WaitForConnection();
				return (Stream)stream;
			});
		}

		[return: MarshalAs(UnmanagedType.Bool)]
		[DllImport("kernel32.dll", CharSet = CharSet.Auto, SetLastError = true)]
		private static extern bool WaitNamedPipe(string name, int timeout);


		/// <summary>
		/// Provides an indication if the named pipe exists. 
		/// This has to prove the pipe does not exist. If there is any doubt, this
		/// returns that it does exist and it is up to the caller to attempt to connect
		/// to that server. The means that there is a wide variety of errors that can occur that
		/// will be ignored - for example, a pipe name that contains invalid characters will result
		/// in a return value of false.
		/// 
		/// </summary>
		/// <param name="pipeName">The pipe to connect to</param>
		/// <returns>false if it can be proven it does not exist, otherwise true</returns>
		/// <remarks>
		/// Attempts to check if the pipe server exists without incurring the cost
		/// of calling NamedPipeClientStream.Connect. This is because Connect either 
		/// times out and throws an exception or goes into a tight spin loop burning
		/// up cpu cycles if the server does not exist.
		/// 
		/// Common Error codes from WinError.h
		/// ERROR_FILE_NOT_FOUND 2L
		/// ERROR_BROKEN_PIPE =  109 (0x6d)
		/// ERROR_BAD_PATHNAME  161L The specified path is invalid.
		/// ERROR_BAD_PIPE =  230  (0xe6) The pipe state is invalid.
		/// ERROR_PIPE_BUSY =  231 (0xe7) All pipe instances are busy.
		/// ERROR_NO_DATA =   232   (0xe8) the pipe is being closed
		/// ERROR_PIPE_NOT_CONNECTED 233L No process is on the other end of the pipe.
		/// ERROR_PIPE_CONNECTED        535L There is a process on other end of the pipe.
		/// ERROR_PIPE_LISTENING        536L Waiting for a process to open the other end of the pipe.
		/// 
		/// </remarks>
		static bool NamedPipeDoesNotExist(string pipeName)
		{
			try
			{
				var timeout = 0;
				var normalizedPath = Path.GetFullPath(string.Format(@"\\.\pipe\{0}", pipeName));
				var exists = WaitNamedPipe(normalizedPath, timeout);
				
				if (!exists)
				{
					var error = Marshal.GetLastWin32Error();
					
					if (error == 0) // pipe does not exist
						return true;
					if (error == 2) // win32 error code for file not found
						return true;
				}
				
				return false;
			}
			catch (Exception)
			{
				// TODO: Report to cortex here
				return true; // assume it exists
			}
		}
	}
}