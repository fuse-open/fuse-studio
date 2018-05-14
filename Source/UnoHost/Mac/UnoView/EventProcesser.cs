using System;
using System.Drawing;
using AppKit;
using CoreGraphics;
using Outracks.UnoHost.OSX.Protocol;

namespace Outracks.UnoHost.OSX.UnoView
{
	static class EventProcesser
	{
		// TODO: move to Fusion.Mac, reverse dependency
		public static CGSize ToSize(this Size<Points> size)
		{
			return new CGSize(size.Width, size.Height);
		}

		public static void SendEvent(NSView view, NSEvent nsEvent)
		{
			switch (nsEvent.Type)
			{
				// Mouse left button
				case NSEventType.LeftMouseDown: view.MouseDown(nsEvent); break;
				case NSEventType.LeftMouseUp: view.MouseUp(nsEvent); break;
				case NSEventType.LeftMouseDragged: view.MouseDragged(nsEvent); break;

				// Mouse right button
				case NSEventType.RightMouseDown: view.RightMouseDown(nsEvent); break;
				case NSEventType.RightMouseUp: view.RightMouseUp(nsEvent); break;
				case NSEventType.RightMouseDragged: view.RightMouseDragged(nsEvent); break;

				// Mouse other button
				case NSEventType.OtherMouseDown: view.OtherMouseDown(nsEvent); break;
				case NSEventType.OtherMouseUp: view.OtherMouseUp(nsEvent); break;
				case NSEventType.OtherMouseDragged: view.OtherMouseDragged(nsEvent); break;

				// Mouse move
				case NSEventType.MouseEntered: view.MouseEntered(nsEvent); break;
				case NSEventType.MouseMoved: view.MouseMoved(nsEvent); break;
				case NSEventType.MouseExited: view.MouseExited(nsEvent); break;

				// Mouse scroll
				case NSEventType.ScrollWheel: view.ScrollWheel(nsEvent); break;

				// Keys
				case NSEventType.KeyDown: view.KeyDown(nsEvent); break;

				case NSEventType.KeyUp: view.KeyUp(nsEvent); break;
			}
		}
	}
}
