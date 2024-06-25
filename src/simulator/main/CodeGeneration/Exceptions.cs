namespace Outracks.Simulator.CodeGeneration
{
	class MissingAppTag : BuildFailed
	{
		public override string Message
		{
			get { return "Couldn't find an App tag in any of the included UX files. Have you forgot to include the UX file that contains the app tag?"; }
		}
	}
}