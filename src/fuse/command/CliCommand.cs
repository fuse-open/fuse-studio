using System.Threading;

namespace Outracks.Fuse
{
	public abstract class CliCommand
	{
		public readonly string Name;
		public readonly string Description;
		public readonly bool IsSecret;
		protected CliCommand(string name, string description, bool isSecret = false)
		{
			Name = name;
			Description = description;
			IsSecret = isSecret;
		}

		public abstract void Help();


		/// <exception cref="ExitWithError" />
		public abstract void Run(string[] args, CancellationToken ct);

	}

	public abstract class DefaultCliCommand : CliCommand
	{
		protected DefaultCliCommand(string name, string description, bool isSecret = false) : base(name, description, isSecret) {}

		public abstract void RunDefault(string[] args, CancellationToken ct);
	}
}