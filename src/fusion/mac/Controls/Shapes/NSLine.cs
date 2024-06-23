using System;
using System.Linq;
using AppKit;
using CoreGraphics;

namespace Outracks.Fusion.Mac
{
	class NSLine : NSDefaultView
	{
		public NSLine()
		{
		}

		public NSLine(IntPtr handle) : base(handle)
		{
		}

		NSColor _strokeColor = NSColor.Clear;
		public NSColor StrokeColor
		{
			get { return _strokeColor; }
			set
			{
				_strokeColor = value;
				NeedsDisplay = true;
			}
		}

		nfloat _lineThickness = 0;
		public nfloat LineThickness
		{
			get { return _lineThickness; }
			set
			{
				_lineThickness = value;
				NeedsDisplay = true;
			}
		}

		nfloat[] _lineDash = new nfloat[0];
		public nfloat[] LineDash
		{
			get { return _lineDash; }
			set
			{
				_lineDash = value;
				NeedsDisplay = true;
			}
		}

		CGPoint _start;
		public CGPoint Start
		{
			get { return _start; }
			set
			{
				_start = value;
				NeedsDisplay = true;
			}
		}

		CGPoint _end;
		public CGPoint End
		{
			get { return _end; }
			set
			{
				_end = value;
				NeedsDisplay = true;
			}
		}

		public override void DrawRect(CGRect dirtyRect)
		{
			base.DrawRect(dirtyRect);

			StrokeColor.SetStroke();
			var path = new NSBezierPath();
			path.MoveTo(Start);
			path.LineTo(End);

			path.LineWidth = LineThickness;
			if (LineDash.Length > 0)
				path.SetLineDash(LineDash.Select(w => w * LineThickness).ToArray(), 0);
			path.Stroke();
		}
	}
}
