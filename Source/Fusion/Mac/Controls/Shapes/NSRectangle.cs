using System;
using System.Drawing;
using System.Linq;
using AppKit;
using CoreGraphics;

namespace Outracks.Fusion.OSX
{
	class NSRectangle : NSShape
	{
		public NSRectangle()
		{			
		}

		public NSRectangle(IntPtr handle) : base(handle)
		{		
		}

		CornerRadius _cornerRadius = CornerRadius.None;
		public CornerRadius CornerRadius
		{
			get { return _cornerRadius; }
			set
			{
				_cornerRadius = value;
				NeedsDisplay = true;
			}
		}

		public override void DrawRect(CGRect dirtyRect)
		{
			base.DrawRect(dirtyRect);

			var rectFactory = CreateRectFactory(CornerRadius);
			if (!IsHidden(FillColor))
			{
				FillColor.SetFill();
				rectFactory(Bounds).Fill();
			}

			if(LineThickness > 0 && !IsHidden(StrokeColor))
			{
				StrokeColor.SetStroke();
				var halfLineThickness = LineThickness * 0.5f;
				var path = rectFactory(CGRect.Inflate(Bounds, -halfLineThickness, -halfLineThickness));
				path.LineWidth = LineThickness;
				if(LineDash.Length > 0) // The API doesn't expect an empty line dash array (for some reason)
					path.SetLineDash(LineDash.Select(w => w * LineThickness).ToArray(), 0);

				path.Stroke();
			}
		}

		static Func<CGRect, NSBezierPath> CreateRectFactory(CornerRadius cornerRadius)
		{
			if (cornerRadius == CornerRadius.None)
			{
				return NSBezierPath.FromRect;
			}
			else
			{
				return rect => NSBezierPath.FromRoundedRect(rect, (float)cornerRadius.RadiusX, (float)cornerRadius.RadiusY);
			}
		}

		bool IsHidden(NSColor color)
		{
			return color.AlphaComponent < 0.05;
		}
	}
}
