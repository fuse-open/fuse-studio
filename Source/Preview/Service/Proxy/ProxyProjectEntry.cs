using Uno.ProjectFormat;

namespace Fuse.Preview
{
	class ProxyProjectEntry
	{
		public int Port { get; set; }
		public string Code { get; set; }
	
		public Project Project { get; set; }
		public string[] Defines { get; set; }
	}
}