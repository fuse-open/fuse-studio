using System;
using System.Drawing;
using System.Reactive.Concurrency;
using System.Reactive.Linq;
using System.Reactive.Subjects;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using Brushes = System.Windows.Media.Brushes;

namespace Outracks.Fusion.Windows
{
	static class DraggingImplementation
	{
		public static void Initialize(Dispatcher dispatcher)
		{
			Dragging.Implementation.OnDragOver = (control, canDrop, drop, enter, leave) =>
			{
				var layer = Layout.Layer(control).WithSize(control.DesiredSize);

				Task.Run(async () =>
				{
					var view = (Canvas)await Fusion.Application.MainThread.InvokeAsync(() => layer.NativeHandle);


					Func<object, bool> canDropNow = o => false;
					layer.BindNativeProperty(Fusion.Application.MainThread, "canDrop", canDrop, c => canDropNow = c);

					Fusion.Application.MainThread.Schedule(() =>
					{
						view.AllowDrop = true;
						view.Background = Brushes.Transparent;
						
						view.Drop += (s, a) =>
						{
							if (a.Data.GetDataPresent(typeof(DragData)))
							{
								var d = (DragData)a.Data.GetData(typeof(DragData));
								if (!canDropNow(d.Data))
									return;

								
								drop(d.Data);
								a.Handled = true;
							}
						};

						var data = Optional.None<Object>();

						view.DragLeave += (s, a) => data.Do(leave);

						view.DragOver += (s, a) =>
						{
							if (a.Data.GetDataPresent(typeof(DragData)))
							{
								var d = (DragData)a.Data.GetData(typeof(DragData));
								if (!canDropNow(d.Data))
									return;

								data = d.Data;
								a.Effects = DragDropEffects.All;
								a.Handled = true;
								data.Do(enter);
							}
						};

					});
				});

				return layer;
			};
		}

		public class DragData
		{
			public readonly object Data;

			public DragData(object data)
			{
				Data = data;
			}
		}
	}
}