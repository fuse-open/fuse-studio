using System;﻿
using AppKit;
using System.Drawing;
using CoreGraphics;

namespace Outracks.UnoHost.OSX.FusionSupport
{
	class EventIntercepter
	{
		public static NSEvent TransformLocationOfEvent(NSView view, NSEvent nsEvent)
		{
			if(!IsMouseEvent(nsEvent.Type))
				return nsEvent;

			var newLocation = view.ConvertPointFromView(nsEvent.LocationInWindow, null);
			if(IsMouseEnterExitEvent(nsEvent.Type))
				return CreateMouseEnterExitEvent(nsEvent, newLocation);
	
			return NSEvent.MouseEvent(
				nsEvent.Type,
				newLocation,
				nsEvent.ModifierFlags,
				nsEvent.Timestamp,
				nsEvent.WindowNumber,
				null,
				nsEvent.EventNumber,
				nsEvent.ClickCount,
				nsEvent.Pressure);
		}

		static NSEvent CreateMouseEnterExitEvent(NSEvent nsEvent, CGPoint location)
		{
			return NSEvent.EnterExitEvent(
				nsEvent.Type, 
				location, 
				nsEvent.ModifierFlags, 
				nsEvent.Timestamp,
				nsEvent.WindowNumber,
				null,
				nsEvent.EventNumber,
				nsEvent.TrackingNumber,
				IntPtr.Zero);
		}

		static bool IsMouseEvent(NSEventType type)
		{
			switch (type)
			{
				case NSEventType.LeftMouseDown:
				case NSEventType.LeftMouseUp:
				case NSEventType.LeftMouseDragged:
				case NSEventType.RightMouseDown:
				case NSEventType.RightMouseUp:
				case NSEventType.RightMouseDragged:
				case NSEventType.OtherMouseDown:
				case NSEventType.OtherMouseUp:
				case NSEventType.OtherMouseDragged:
				case NSEventType.MouseEntered:
				case NSEventType.MouseMoved:
				case NSEventType.MouseExited:
					return true;
				default:
					return false;
			}
		}
		
		static bool IsMouseEnterExitEvent(NSEventType type)
		{
			switch(type)
			{
				case NSEventType.MouseEntered:
				case NSEventType.MouseExited:
					return true;
				default:
					return false;
			}
		}
	}
}
