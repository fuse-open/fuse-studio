using System;
using System.Reactive.Concurrency;
using System.Reactive.Linq;
using Fuse.Preview;

namespace Outracks.Fuse
{
	using AndroidManager;
	using Fusion;

	class USBMode
	{
		readonly AndroidPortReverser _portReverser;
		readonly IObservable<int> _port;
		readonly PreviewService _previewService;

		public Command EnableUsbModeCommand
		{
			get
			{
				return _port.Switch(p =>
					Command.Enabled(action: () =>
					{
						EnableUsbMode(p);
					}));
			}
		}

		public USBMode(AndroidPortReverser portReverser, IObservable<int> port, PreviewService previewService)
		{
			_portReverser = portReverser;
			_port = port;
			_previewService = previewService;
			_port.ObserveOn(NewThreadScheduler.Default).Subscribe(EnableUsbMode);
		}

		void EnableUsbMode(int port)
		{
			_portReverser.ReversePortOrLogErrors(ReportFactory.FallbackReport, port, port);
			_previewService.UpdateReversedPorts(true);
		}
	}
}