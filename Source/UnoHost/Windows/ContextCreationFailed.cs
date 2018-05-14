using System;

namespace Outracks.UnoHost.Windows
{
	class ContextCreationFailed : Exception
	{
		public ContextCreationFailed(string functionName, uint errorCode)
			: base(functionName + " returned error code " + errorCode)
		{
			
		}
		public ContextCreationFailed(string message)
			: base("Failed to create OpenGL context ("+message+")")
		{

		}


		public ContextCreationFailed(Exception innerException) 
			: base("Failed to create OpenGL context", innerException)
		{
			
		}
	}
}