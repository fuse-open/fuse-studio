using System.Reactive.Subjects;

namespace Outracks.Fuse.Stage
{
	using Fusion;
	using Simulator.Bytecode;

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