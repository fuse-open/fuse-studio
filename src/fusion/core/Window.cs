using System;
using System.Reactive.Linq;

namespace Outracks.Fusion
{
	public enum WindowState
	{
		Normal,
		Maximized
	}

	public enum WindowStyle
	{
		Regular, Sheet, None, Fat
	}

	public class Window
	{
		public WindowStyle Style { get; set; }
		public IObservable<string> Title { get; set; }
		public Optional<IObservable<Icon>> Icon { get; set; }

		public Optional<Menu> Menu { get; set; }
		public IControl Content { get; set; }

		public Command Closed { get; set; }

		public Optional<IProperty<Optional<Size<Points>>>> Size { get; set; }
		public Optional<IProperty<Optional<Point<Points>>>> Position { get; set; }
		public Optional<IProperty<Optional<WindowState>>> State { get; set; }

		public Optional<IObservable<bool>> TopMost { get; set; }
		public Optional<IObservable<bool>> HideMenu { get; set; }

		public Optional<DropOperation> DragOperation { get; set; }
		public Command Focused { get; set; }

		public Stroke Border { get; set; }
		public Brush Background { get; set; }
		public Brush Foreground { get; set; }
		public bool HideTitle { get; set; }

		public Window()
		{
			Style = WindowStyle.Regular;
			Title = Observable.Return("");
			Content = Control.Empty;
			Closed = Command.Disabled;
			Focused = Command.Disabled;
			Border = Stroke.Empty;
		}
	}
}
