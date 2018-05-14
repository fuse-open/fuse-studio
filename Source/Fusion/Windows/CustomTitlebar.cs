using System.Windows;
using System.Windows.Input;

namespace Outracks.Fusion.Windows
{
	public static class CustomTitlebar
    {
        public static void ApplyTo(System.Windows.Window window)
        {
	        CanExecuteRoutedEventHandler onCanResizeWindow = (sender, e) =>
	        {
				e.CanExecute = 
					window.ResizeMode == ResizeMode.CanResize || 
					window.ResizeMode == ResizeMode.CanResizeWithGrip;
	        };

			CanExecuteRoutedEventHandler onCanMinimizeWindow = (sender, e) => 
			{
				e.CanExecute = window.ResizeMode != ResizeMode.NoResize;
			};

			ExecutedRoutedEventHandler onCloseWindow = (sender, e) => SystemCommands.CloseWindow(window);
			ExecutedRoutedEventHandler onMaximizeWindow = (sender, e) =>
			{
				SystemCommands.MaximizeWindow(window);

			};
			ExecutedRoutedEventHandler onMinimizeWindow = (sender, e) => SystemCommands.MinimizeWindow(window);
			ExecutedRoutedEventHandler onRestoreWindow = (sender, e) =>
			{
				SystemCommands.RestoreWindow(window);
			};

			window.CommandBindings.Add(new CommandBinding(SystemCommands.CloseWindowCommand, onCloseWindow));
			window.CommandBindings.Add(new CommandBinding(SystemCommands.MaximizeWindowCommand, onMaximizeWindow, onCanResizeWindow));
			window.CommandBindings.Add(new CommandBinding(SystemCommands.MinimizeWindowCommand, onMinimizeWindow, onCanMinimizeWindow));
			window.CommandBindings.Add(new CommandBinding(SystemCommands.RestoreWindowCommand, onRestoreWindow, onCanResizeWindow));
        }

    }
}
