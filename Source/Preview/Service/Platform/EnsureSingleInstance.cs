using System;
using System.IO;
using System.Threading;
using Outracks;

namespace Fuse.Preview
{
	public interface IEnsureSingleInstance : IDisposable
	{
		bool IsAlreadyRunning();
	}

	class EnsureSingleInstanceWin : IEnsureSingleInstance
	{
		readonly Mutex _mutex;
		readonly bool _createdNew;

		public EnsureSingleInstanceWin(string name)
		{
			_mutex = new Mutex(true, name, out _createdNew);
		}

		public bool IsAlreadyRunning()
		{
			return !_createdNew;
		}

		public void Dispose()
		{
			if (_createdNew)
				_mutex.ReleaseMutex();

			_mutex.Dispose();
		}
	}

	class EnsureSingleInstanceMac : IEnsureSingleInstance
	{
		readonly FileStream _stream;
		readonly bool _isSingle = true;
		readonly IReport _report;

		const int FileLocked = -2147024863;

		public EnsureSingleInstanceMac(string name, IReport report)
		{
			_report = report;
			_stream = File.OpenWrite("/tmp/" + name + Environment.UserDomainName + "_" + Environment.UserName);
			try
			{
				_stream.Lock(0, _stream.Length);
				_report.Info("Successfully locked");
			}
			catch (IOException e)
			{
				if (e.HResult == FileLocked)
					_isSingle = false;
				else
					throw e;
			}
		}

		public bool IsAlreadyRunning()
		{
			return !_isSingle;
		}

		public void Dispose()
		{
			_report.Info("Disposing lock");
			_stream.Unlock(0, _stream.Length);
			_stream.Dispose();
		}
	}
}