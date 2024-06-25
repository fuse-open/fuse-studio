using System;
using System.Diagnostics;
using System.Runtime.InteropServices;
using System.Threading;
using System.Windows.Forms;

namespace Outracks.UnoHost.Windows
{
	public interface IVSyncContext
	{
		void WaitForVBlank();
	}

	public class SimulatedVSync : IVSyncContext
	{
		readonly Stopwatch _stopWatch = new Stopwatch();
		public void WaitForVBlank()
		{
			var elapsedTime = _stopWatch.ElapsedMilliseconds;
			if (elapsedTime < 16)
			{
				Thread.Sleep((int)(16 - elapsedTime));
			}
			_stopWatch.Restart();
		}
	}

	public class VsyncContext : IVSyncContext
	{
		D3DKMT_WAITFORVERTICALBLANKEVENT _waitforverticalblankevent;

		internal VsyncContext(D3DKMT_WAITFORVERTICALBLANKEVENT waitforverticalblankevent)
		{
			_waitforverticalblankevent = waitforverticalblankevent;
		}

		public void WaitForVBlank()
		{
			D3DKMTWaitForVerticalBlankEvent(ref _waitforverticalblankevent);
		}

		[DllImport("gdi32.dll")]
		static extern uint D3DKMTWaitForVerticalBlankEvent(ref D3DKMT_WAITFORVERTICALBLANKEVENT pData);
	}

	public static class VerticalSynchronization
	{
		/// <summary>
		/// Make sure to call this after a Window context is made!
		/// </summary>
		/// <returns></returns>
		public static IVSyncContext CreateContext()
		{
			try
			{
				var openAdapter = new D3DKMT_OPENADAPTERFROMHDC();
				var waitForVblankEvent = new D3DKMT_WAITFORVERTICALBLANKEVENT();

				var hdc = CreateDC(Screen.PrimaryScreen.DeviceName, null, null, IntPtr.Zero);
				openAdapter.hDc = hdc;
				var result = D3DKMTOpenAdapterFromHdc(ref openAdapter);
				if (result != 0)
					throw new Exception("D3DKMTOpenAdapterFromHdc failed");

				waitForVblankEvent.hAdapter = openAdapter.hAdapter;
				waitForVblankEvent.hDevice = 0;
				waitForVblankEvent.VidPnSourceId = openAdapter.VidPnSourceId;

				return new VsyncContext(waitForVblankEvent);
			}
			catch (Exception e)
			{
				ReportFactory.FallbackReport.Error(e);
			}

			return new SimulatedVSync();
		}

		[DllImport("gdi32.dll")]
		static extern IntPtr CreateDC(string lpszDriver, string lpszDevice,
		   string lpszOutput, IntPtr lpInitData);

		[DllImport("gdi32.dll")]
		static extern uint D3DKMTOpenAdapterFromHdc(ref D3DKMT_OPENADAPTERFROMHDC pData);
	}

	struct D3DKMT_OPENADAPTERFROMHDC
	{
		public IntPtr hDc;
		public uint hAdapter;
		public uint AdapterLuidLowPart;
		public uint AdapterLuidHighPart;
		public uint VidPnSourceId;
	}

	struct D3DKMT_WAITFORVERTICALBLANKEVENT
	{
		public uint hAdapter;
		public uint hDevice;
		public uint VidPnSourceId;
	}
}