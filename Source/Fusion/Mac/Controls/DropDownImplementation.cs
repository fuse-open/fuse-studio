using System;
using System.Reactive.Concurrency;
using System.Reactive.Linq;
using AppKit;
using CoreGraphics;

namespace Outracks.Fusion.OSX
{
	class DropDownImplementation
	{
		public static void Initialize(IScheduler dispatcher)
		{
			DropDown.Implementation.Factory = (value, items, nativeLook) => 
				Control.Create(mountLocation =>
				{
					value = value
						.ConnectWhile(mountLocation.IsRooted)
						.AutoInvalidate();

					var view = new NSPopUpButton();
					var c = nativeLook ? new NSPopUpButtonCell()
					{
						BezelStyle = NSBezelStyle.Rounded,
					} : new CustomPopUpButtonCell();
					view.Cell = c;
					
					mountLocation.BindNativeDefaults(view, dispatcher);

					// Disable control if list is empty
					var isDisabled = value.IsReadOnly.Or(items.Select(x => x.IsEmpty()));
					mountLocation.BindNativeProperty(dispatcher, "Enabled", isDisabled, isReadOnly => view.Enabled = !isReadOnly);

					mountLocation.BindNativeProperty(dispatcher, "items", items, tmpItems =>
					{
						view.RemoveAllItems();
						foreach (var i in tmpItems)
						{
							var tmp = i;
							view.Menu.AddItem(new NSMenuItem(i.ToString(), (s, a) => value.Write(tmp)));
						}
					});

					return view;
				})
				.WithHeight(30);
				

		}
	}

	class CustomPopUpButtonCell : NSPopUpButtonCell
	{
		public CustomPopUpButtonCell()
		{			
		}

		public CustomPopUpButtonCell(IntPtr handle) : base(handle)
		{			
		}

		public override void DrawBezelWithFrame(CGRect frame, NSView controlView)
		{
		}
	}
}