using System;
using System.Drawing;
using System.Linq;
using AppKit;

namespace Outracks.Fusion.OSX
{
	class NSShape : NSDefaultView
	{
		public NSShape()
		{			
		}

		public NSShape(IntPtr handle) : base(handle)
		{			
		}

		NSColor _fillColor = NSColor.Clear;
		public NSColor FillColor
		{
			get { return _fillColor; }
			set
			{
				_fillColor = value;
				//if(_fillColor.AlphaComponent < 1)
					//_isOpaque = false;
					
				NeedsDisplay = true;
			}
		}
		
		//bool _isOpaque = true;
		public override bool IsOpaque
		{
			get 
			{
				return false;
			}
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
	}
}
