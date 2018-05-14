using System.Collections.Generic;
using System.Linq;
using Fuse.Preview;
using System.Net;
using System.Reactive.Linq;
using Outracks.Fusion;
using QRCoder;

namespace Outracks.Fuse
{
	class CodeView
	{
		readonly Code _code;
		readonly Optional<IPAddress> _endpoint;
		readonly IControl _qrCode;

		public CodeView(Code code, IPAddress[] endpoints)
		{
			_code = code;
			_endpoint = endpoints.Where(address => address.Equals(IPAddress.Loopback) == false).LastOrNone();
			_qrCode = CreateQrCode(CreateCodeQrData(endpoints, code));
		}

		static string CreateCodeQrData(IEnumerable<IPAddress> endpoints, Code code)
		{
			return string.Join("\n", endpoints) + "\nCode:" + code;
		}

		public IControl Create(IPopover popover)
		{
			return Create(popover, _code, _endpoint, _qrCode);
		}

		public static IControl Create(IPopover popover, Code code, Optional<IPAddress> endpoint, IControl qrCode)
		{
			return popover.CreatePopover(
				RectangleEdge.Top,
				state =>
				{
					return 
						Button.Create(state.IsVisible.Toggle(), s =>
							Layout.StackFromLeft(
									Control.Empty.WithWidth(4),
									Icons.DevicesIcon(),
									Control.Empty.WithWidth(4),
									Label.Create(
											text: "Devices",
											color: Theme.DefaultText,
											font: Theme.DescriptorFont)
										.CenterVertically(),
									Control.Empty.WithWidth(4))
								.SetToolTip("Connect this project to a device with the Fuse Preview App installed on it.")
								.WithBackground(
									background: Observable.CombineLatest(
											s.IsEnabled, s.IsHovered,
											(enabled, hovering) =>
												hovering
													? Theme.FaintBackground
													: Color.Transparent)
										.Switch()));
				},
				state =>
				{
					return Layout.StackFromTop(
						qrCode,
						TextView("IP address:", endpoint.Select(e => e.ToString()).Or("Failed to find IP")),
						TextView("Code:", code.ToString())
					)
					.WithWidth(200)
					.CenterHorizontally()
					.WithPadding(new Thickness<Points>(0,8,0,16))
					.WithWidth(250);
				});
		}

		static IControl TextView(string section, string data)
		{
			return Layout.StackFromTop(
					Label.Create(section, color: Theme.DefaultText).WithPadding(new Thickness<Points>(0, 8)),
					Label.Create(data, font: Font.SystemDefault(20), color: Color.FromRgb(0x2897B1), textAlignment: TextAlignment.Center)
					.WithPadding(new Thickness<Points>(16, 8))
					.WithOverlay(
						Shapes.Rectangle(
							stroke: Stroke.Create(1, Theme.LineBrush),
							fill: Brush.Transparent,
							cornerRadius: Observable.Return(new CornerRadius(2)))));
		}

		static IControl CreateQrCode(string data)
		{
			var qrGenerator = new QRCodeGenerator();
			var qrCodeData = qrGenerator.CreateQrCode(data, QRCodeGenerator.ECCLevel.Q);
			var qrCode = new QRCode(qrCodeData);
			var pixelSize = 3;
			return Image.FromBitmaps(new[] { qrCode.GetGraphic(pixelSize), qrCode.GetGraphic(2 * pixelSize) });
		}
	}
}