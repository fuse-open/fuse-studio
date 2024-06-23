using System;

namespace Fuse.Preview
{
	class ProxyServerFailed : Exception
	{
		public ProxyServerFailed(string message) : base(message) { }
	}
}