using System.Reactive.Subjects;
using Outracks.Fusion;
using Outracks.Simulator.Bytecode;

namespace Outracks.Fuse.Stage
{
	public interface IViewport
	{
		/// <summary>
		/// Execute bytecode remotly.
		/// </summary>
		void Execute(Statement bytecode);

		BehaviorSubject<VirtualDevice> VirtualDevice { get; }

		IControl Control { get; }

		void Close();

		void Focus();
	}
}