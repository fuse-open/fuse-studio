using System;
using System.Collections.Generic;
using System.Reactive.Concurrency;
using System.Reactive.Linq;
using System.Text;
using AppKit;

namespace Outracks.Fusion.Mac
{
	static class LayeringImplementation
	{
		public static void DebugWriteNSTree(NSView childContainer)
		{
			var root = childContainer;
			while (root.Superview != null)
			{
				root = root.Superview;
			}

			var sb = new StringBuilder();
			Stack<Tuple<NSView,int>> toVisit = new Stack<Tuple<NSView,int>>();

			toVisit.Push(Tuple.Create(root,0));
			while (!toVisit.IsEmpty())
			{
				var current = toVisit.Pop();

				sb.Append('\t',current.Item2);
				sb.Append(current.Item1 + " : " + current.Item1.Frame + "\n");

				foreach (var chi in current.Item1.Subviews)
					toVisit.Push(Tuple.Create(chi,current.Item2+1));
			}

			Console.Write(sb.ToString());
		}

		public static void Initialize(IScheduler dispatcher)
		{
			Layout.Implementation.LayerControls = (childFactory) =>
				Control.Create(ctrl =>
				{
					var element = new NSView();
					element.Hidden = true;

					ctrl.BindNativeDefaults(element, dispatcher);

					childFactory(ctrl)
						// IMPORTANT: ConnectWhile has to be done first since else we'll lose all changes done to the children list while the ctrl is unrooted.
						// which breaks diffing, and we get dangling views (views that aren't removed).
						.ConnectWhile(ctrl.IsRooted)
						.DiffSequence()
						.ObserveOn(Fusion.Application.MainThread)
						.Subscribe(children =>
						{
							element.Hidden = children.Current.Count == 0;
							foreach (var child in children.Removed)
							{
								var nativeChild = child.NativeHandle as NSView;
								if (nativeChild != null)
									nativeChild.RemoveFromSuperview();

								child.Mount(MountLocation.Unmounted);
							}

							foreach (var child in children.Added)
							{
								child.Mount(
									new MountLocation.Mutable
									{
										IsRooted = ctrl.IsRooted,
										AvailableSize = ctrl.AvailableSize,
										NativeFrame = ObservableMath.RectangleWithSize(ctrl.NativeFrame.Size),
									});

								var nativeChild = child.NativeHandle as NSView;
								if (nativeChild == null)
									continue;

								if (nativeChild.Superview != null)
									nativeChild.RemoveFromSuperview();

								NSView nativeChildBelow = null;
								for (int i = children.Current.IndexOf(child) - 1; i >= 0; i--)
								{
									nativeChildBelow = children.Current[i].NativeHandle as NSView;
									if (nativeChildBelow != null)
										break;
								}

								if (nativeChildBelow != null)
									element.AddSubview(nativeChild, NSWindowOrderingMode.Above, nativeChildBelow);
								else
									element.AddSubview(nativeChild);

							}
						});

					return element;
				});
		}
	}
}
