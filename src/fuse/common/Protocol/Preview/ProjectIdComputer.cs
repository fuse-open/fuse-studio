using System;
using System.Security.Cryptography;
using System.Text;
using Outracks.IO;

namespace Outracks.Fuse.Protocol.Preview
{
	public static class ProjectIdComputer
	{
		public static Guid IdFor(IAbsolutePath projectPath)
		{
			using (var md5Hash = MD5.Create())
			{
				var data = md5Hash.ComputeHash(Encoding.UTF8.GetBytes(projectPath.NativePath));
				var sBuilder = new StringBuilder();
				foreach (var t in data)
				{
					sBuilder.Append(t.ToString("x2"));
				}
				return new Guid(sBuilder.ToString());
			}
		}
	}
}
