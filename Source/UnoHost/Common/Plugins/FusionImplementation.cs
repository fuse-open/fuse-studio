using System;
using System.Collections.Generic;
using System.Linq;
using System.Reactive.Concurrency;
using System.Reactive.Linq;
using Outracks.IO;
using Outracks.Simulator.Client;
using Outracks.Simulator.Runtime;
using Uno;

namespace Outracks.UnoHost
{
	using Fusion;

	public static class FusionImplementation
	{
		static dynamic _reflection;

		public static void Initialize(IScheduler mainThreadDispatcher, AbsoluteFilePath userData, dynamic reflection)
		{
			UserSettings.Settings = PersistentSettings.Load(usersettingsfile: userData);

			Axis2DExtensions.ShouldFlip = false;
			Application.MainThread = mainThreadDispatcher;
			_reflection = reflection;
			

			Pointer.Implementation.MakeHittable = (control, space, callbacks) =>
			{
				var o = control.NativeHandle;

				reflection.CallStatic("Outracks.UnoHost.FusionInterop", "OnPointerPressed", o, new Action<Float2>(pos =>
					callbacks.OnPressed(new Pointer.OnPressedArgs(new Point<Points>(pos.X, pos.Y), 1))));

				reflection.CallStatic("Outracks.UnoHost.FusionInterop", "OnPointerMoved", o, new Action<Float2>(pos =>
					callbacks.OnMoved(new Point<Points>(pos.X, pos.Y))));

				return control;
			};

			Layout.Implementation.LayerControls = (childFactory) =>
				Control.Create(self =>
				{
					var control = reflection.Instantiate("Fuse.Controls.Panel");

					BindNativeDefaults(self, control);

					var element = control as dynamic;

					self.BindList(
						list: childFactory(self).Select(Enumerable.Reverse),
						add: child =>
						{
							child.Mount(
								new MountLocation.Mutable
								{
									IsRooted = self.IsRooted,
									AvailableSize = self.AvailableSize,
									NativeFrame = ObservableMath.RectangleWithSize(self.NativeFrame.Size),
								});

							var nativeChild = child.NativeHandle as dynamic;
							if (nativeChild != null)
								element.Children.Add(nativeChild);
						},
						remove: child =>
						{
							var nativeChild = child.NativeHandle as dynamic;
							if (nativeChild != null)
								element.Children.Remove(nativeChild);

							child.Mount(MountLocation.Unmounted);
						});

					return control;
				});

			Shapes.Implementation.RectangleFactory = (stroke, brush, cornerRadius) =>
				Control.Create(self =>
				{
					var control = _reflection.Instantiate("Fuse.Controls.Rectangle");

					BindNativeDefaults(self, control);
					BindShapeProperties(self, control, brush, stroke);

					return control;
				});
		}

		public static void BindShapeProperties(this IMountLocation self, object obj, Brush fill, Stroke stroke)
		{
			var control = obj as dynamic;

			control.Stroke = _reflection.Instantiate("Fuse.Drawing.Stroke") as dynamic;
			self.BindNativeProperty(fill, f => control.Color = new Float4(f.R, f.G, f.B, f.A));
			self.BindNativeProperty(stroke.Brush.CombineLatest(stroke.DashArray),
				t =>
				{
					var s = t.Item1;
					var dashArray = t.Item2;
					//Reflection.CallStatic("Fuse.Diagnostics","UserWarning", "Setting stroke of " + obj.GetHashCode() + " to " + s, obj, null, 0, null);
					if (dashArray == StrokeDashArray.Solid)
					{
						control.Stroke.Color = new Float4(s.R, s.G, s.B, s.A);
					}
					else
					{
						if (!_reflection.IsSubtype(control.Stroke.Brush, "DashedSolidColor"))
							control.Stroke.Brush = _reflection.Instantiate("DashedSolidColor", new Float4(s.R, s.G, s.B, s.A)) as dynamic;
						else
							control.Stroke.Brush.Color = new Float4(s.R, s.G, s.B, s.A);

						control.Stroke.Brush.DashSize = (float)dashArray.Data[0];
					}
				});
			self.BindNativeProperty(stroke.Thickness, s => control.Stroke.Width = (float)s);
		}

		public static void BindNativeDefaults(this IMountLocation self, object obj)
		{
			var element = obj as dynamic;

			var frame = self.NativeFrame;

			self.BindNativeProperty(frame.Left(), left =>
				element.X = _reflection.CallStatic("Uno.UX.Size", "Points", (float)left) as dynamic);
			self.BindNativeProperty(frame.Top(), top =>
				element.Y = _reflection.CallStatic("Uno.UX.Size", "Points", (float)top) as dynamic);
			self.BindNativeProperty(frame.Height, height =>
				element.Height = _reflection.CallStatic("Uno.UX.Size", "Points", (float)height) as dynamic);
			self.BindNativeProperty(frame.Width, width =>
				element.Width = _reflection.CallStatic("Uno.UX.Size", "Points", (float)width) as dynamic);
		}

		public static void BindNativeProperty<TValue>(
			this IMountLocation control,
			IObservable<TValue> value,
			Action<TValue> update)
		{
			value
				.ConnectWhile(control.IsRooted)
				.DistinctUntilChanged()
				.Subscribe(update);
		}

		public static void BindList<T>(
			this IMountLocation self,
			IObservable<IEnumerable<T>> list,
			Action<T> add,
			Action<T> remove)
		{
			list
				// IMPORTANT: ConnectWhile has to be done first since else we'll lose all changes done to the children list while the ctrl is unrooted.
				// which breaks diffing, and we get dangling views (views that aren't removed).
				.ConnectWhile(self.IsRooted)
				.DiffSequence()
				.Subscribe(children =>
				{
					foreach (var child in children.Removed)
					{
						remove(child);
					}

					foreach (var child in children.Added)
					{
						add(child);
					}
				});
		}

	}
}