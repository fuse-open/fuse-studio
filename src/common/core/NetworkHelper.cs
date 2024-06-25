using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Net;
using System.Net.NetworkInformation;
using System.Net.Sockets;
using System.Reactive;
using System.Text;
using System.Text.RegularExpressions;
using Outracks.Diagnostics;

namespace Outracks.IPC
{
	public class NetworkHelper
	{
		/// <exception cref="FailedToLoadTcpTable"></exception>
		public static bool IsPortOpen(int port)
		{
			if (Platform.IsWindows)
				return IsPortOpenWin(port);
			else if(Platform.IsMac)
				return IsPortOpenOSX(port);

			throw new PlatformNotSupportedException();
		}

		static bool IsPortOpenOSX(int port)
		{
			var lsofStartupInfo = new ProcessStartInfo("sh")
			{
				UseShellExecute = false,
				Arguments = "-c \"lsof -i tcp:" + port + " | grep LISTEN\""
			};

			var p = Process.Start(lsofStartupInfo);
			p.WaitForExit();
			return p.ExitCode == 1;
		}

		static bool IsPortOpenWin(int port)
		{
			try
			{
				var isAvailable = true;

				// Evaluate current system tcp connections. This is the same information provided
				// by the netstat command line application, just in .Net strongly-typed object
				// form.  We will look through the list, and if our port we would like to use
				// in our TcpClient is occupied, we will set isAvailable to false.
				var ipGlobalProperties = IPGlobalProperties.GetIPGlobalProperties();
				var tcpConnInfoArray = ipGlobalProperties.GetActiveTcpListeners();

				foreach (var tcpi in tcpConnInfoArray)
				{
					if (tcpi.Port == port)
					{
						isAvailable = false;
						break;
					}
				}

				return isAvailable;
			}
			catch (NetworkInformationException e)
			{
				throw new FailedToLoadTcpTable(e);
			}
		}

		/// <exception cref="UnableToResolveHostNameException"></exception>
		public static IEnumerable<IPAddress> GetInterNetworkIps()
		{
			try
			{
				var hostNameOrAddress = Dns.GetHostName();
				return GetIP(hostNameOrAddress);
			}
			catch (Exception e)
			{
				if (Platform.IsMac)
					return GetIPFallbackSafe();
				else
					throw new UnableToResolveHostNameException(e);
			}
		}

		static IEnumerable<IPAddress> GetIP(string hostNameOrAddress)
		{
			var addressList = Dns.GetHostEntry(hostNameOrAddress).AddressList;
			return
				new[] { IPAddress.Loopback }
					.Union(
						addressList
							.Where(ip => ip.AddressFamily == AddressFamily.InterNetwork)
							.Reverse());
		}

		static IEnumerable<IPAddress> GetIPFallbackSafe()
		{
			try
			{
				return GetIpFallback();
			}
			catch (Exception e)
			{
				throw new UnableToResolveHostNameException(e);
			}
		}

		static IEnumerable<IPAddress> GetIpFallback()
		{
			var ifConfigInfo = new ProcessStartInfo()
			{
				FileName = "ifconfig",
				UseShellExecute = false,
				RedirectStandardOutput = true,
				RedirectStandardError = true
			};

			var ifConfig = Process.Start(ifConfigInfo);
			var output = new StringBuilder();
			var errors = new StringBuilder();

			ifConfig.ConsumeOutAndErr(
				Observer.Create<string>(str => output.Append(str)),
				Observer.Create<string>(str => errors.Append(str)))
				.Wait();

			if (errors.Length > 0)
				throw new IfConfigErrorException(errors.ToString());

			return ExtractIpsFromIfconfigOutput(output.ToString());
		}

		internal static IEnumerable<IPAddress> ExtractIpsFromIfconfigOutput(string ifconfigOutput)
		{
			return Regex.Matches(ifconfigOutput, @"inet\s+(?<ip>\d+\.\d+\.\d+\.\d+)")
				.Cast<Match>()
				.Select(matchResult => IPAddress.Parse(matchResult.Groups["ip"].Value));
		}
	}

	class IfConfigErrorException : Exception
	{
		public IfConfigErrorException(string message)
			: base(message)
		{
		}
	}

	public class UnableToResolveHostNameException : AggregateException
	{
		public UnableToResolveHostNameException()
			: base("Unable to resolve host name.")
		{
		}

		public UnableToResolveHostNameException(Exception innerExceptions)
			: base("Unable to resolve host name.", innerExceptions)
		{
		}
	}

	public class FailedToLoadTcpTable : AggregateException
	{
		public FailedToLoadTcpTable(Exception innerException) : base("Unable to load TCP table information", innerException)
		{
		}
	}
}