using System.Net;
using NUnit.Framework;
using Outracks.IPC;

namespace Outracks.Common.Tests
{
	[TestFixture]
	public class NetworkHelperTests
	{
		[Test]
		public void ExtractIpsFromIfconfigOutput_is_successful()
		{
			var ips = NetworkHelper.ExtractIpsFromIfconfigOutput(_ifconfigOutput);
			CollectionAssert.AreEqual(ips, new[] { IPAddress.Parse("127.0.0.1"), IPAddress.Parse("192.168.1.112") });
		}

		static readonly string _ifconfigOutput = @"	lo0: flags=8049<UP,LOOPBACK,RUNNING,MULTICAST> mtu 16384
	options=1203<RXCSUM,TXCSUM,TXSTATUS,SW_TIMESTAMP>
	inet 127.0.0.1 netmask 0xff000000 
	inet6 ::1 prefixlen 128 
	inet6 fe80::1%lo0 prefixlen 64 scopeid 0x1 
	nd6 options=201<PERFORMNUD,DAD>
gif0: flags=8010<POINTOPOINT,MULTICAST> mtu 1280
stf0: flags=0<> mtu 1280
en0: flags=8863<UP,BROADCAST,SMART,RUNNING,SIMPLEX,MULTICAST> mtu 1500
	options=10b<RXCSUM,TXCSUM,VLAN_HWTAGGING,AV>
	ether ac:87:a3:08:5b:2a 
	inet6 fe80::e0:8f00:3d02:dc62%en0 prefixlen 64 secured scopeid 0x4 
	inet 192.168.1.112 netmask 0xffffff00 broadcast 192.168.1.255
	nd6 options=201<PERFORMNUD,DAD>
	media: autoselect (1000baseT <full-duplex>)
	status: active
en1: flags=8863<UP,BROADCAST,SMART,RUNNING,SIMPLEX,MULTICAST> mtu 1500
	ether 88:63:df:c3:01:77 
	nd6 options=201<PERFORMNUD,DAD>
	media: autoselect (<unknown type>)
	status: inactive
en2: flags=963<UP,BROADCAST,SMART,RUNNING,PROMISC,SIMPLEX> mtu 1500
	options=60<TSO4,TSO6>
	ether 0a:00:00:29:9d:c0 
	media: autoselect <full-duplex>
	status: inactive
en3: flags=963<UP,BROADCAST,SMART,RUNNING,PROMISC,SIMPLEX> mtu 1500
	options=60<TSO4,TSO6>
	ether 0a:00:00:29:9d:c1 
	media: autoselect <full-duplex>
	status: inactive
bridge0: flags=8863<UP,BROADCAST,SMART,RUNNING,SIMPLEX,MULTICAST> mtu 1500
	options=63<RXCSUM,TXCSUM,TSO4,TSO6>
	ether 0a:00:00:29:9d:c0 
	Configuration:
		id 0:0:0:0:0:0 priority 0 hellotime 0 fwddelay 0
		maxage 0 holdcnt 0 proto stp maxaddr 100 timeout 1200
		root id 0:0:0:0:0:0 priority 0 ifcost 0 port 0
		ipfilter disabled flags 0x2
	member: en2 flags=3<LEARNING,DISCOVER>
	        ifmaxaddr 0 port 6 priority 0 path cost 0
	member: en3 flags=3<LEARNING,DISCOVER>
	        ifmaxaddr 0 port 7 priority 0 path cost 0
	nd6 options=201<PERFORMNUD,DAD>
	media: <unknown type>
	status: inactive
p2p0: flags=8843<UP,BROADCAST,RUNNING,SIMPLEX,MULTICAST> mtu 2304
	ether 0a:63:df:c3:01:77 
	media: autoselect
	status: inactive
awdl0: flags=8943<UP,BROADCAST,RUNNING,PROMISC,SIMPLEX,MULTICAST> mtu 1484
	ether 52:d7:0b:c9:85:c5 
	inet6 fe80::50d7:bff:fec9:85c5%awdl0 prefixlen 64 scopeid 0xa 
	nd6 options=201<PERFORMNUD,DAD>
	media: autoselect
	status: active
utun0: flags=8051<UP,POINTOPOINT,RUNNING,MULTICAST> mtu 2000
	inet6 fe80::de69:9795:36ad:6da%utun0 prefixlen 64 scopeid 0xb 
	nd6 options=201<PERFORMNUD,DAD>";
	}
}