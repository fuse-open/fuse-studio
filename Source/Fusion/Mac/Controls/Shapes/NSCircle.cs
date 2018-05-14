using System;
using System.Drawing;
using System.Linq;
using AppKit;
using CoreGraphics;

namespace Outracks.Fusion.OSX
{
	class NSCircle : NSShape
	{
		public NSCircle()
		{			
		}

		public NSCircle(IntPtr handle)
			: base(handle)
		{
		}

		public override void DrawRect(CGRect dirtyRect)
		{
			base.DrawRect(dirtyRect);

			FillColor.SetFill();
			NSBezierPath.FromOvalInRect(Bounds).Fill();

			if(LineThickness > 0)
			{
				StrokeColor.SetStroke();
				var halfLineThickness = LineThickness * 0.5f;
				var path = NSBezierPath.FromOvalInRect(CGRect.Inflate(Bounds, -halfLineThickness, -halfLineThickness));
				path.LineWidth = LineThickness;
				if(LineDash.Length > 0) // The API doesn't expect an empty line dash array (for some reason)
					path.SetLineDash(LineDash.Select(w => w * LineThickness).ToArray(), 0);

				path.Stroke();
			}
		}
	}
}
