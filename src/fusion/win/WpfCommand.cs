using System;
using System.Windows.Input;

namespace Outracks.Fusion.Windows
{
	public class WpfCommand : ICommand
	{
		readonly Optional<Action> _execute;

		public WpfCommand(Optional<Action> handler)
		{
			_execute = handler;
		}

		public bool CanExecute(object parameter)
		{
			return _execute.HasValue;
		}

		public void Execute(object parameter)
		{
			_execute.Do(x => x());
		}
#pragma warning disable
		public event EventHandler CanExecuteChanged;
#pragma warning restore
	}
}