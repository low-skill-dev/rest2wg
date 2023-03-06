using Microsoft.VisualStudio.TestPlatform.Utilities;
using System.Security.Cryptography;
using vdb_node_api.Services;
using vdb_node_wireguard_manipulator;
using Xunit.Abstractions;

namespace vdb_node_api.tests.Services
{
	public class IpDedicationServiceTests
	{
		private ITestOutputHelper _output;
		private IpDedicationService service;
		private Func<string, int> StringToIndex;
		private Func<int, string> IndexToString;
		private Func<Dictionary<string, int>> _dedicatedAddresses;
		public IpDedicationServiceTests(ITestOutputHelper output)
		{
			_output = output;

			service = new();
			var obj = new Microsoft.VisualStudio.TestTools.UnitTesting.PrivateObject(service);

			IndexToString = (arg)
				=> (string)obj.Invoke("IndexToString", new object[1] { arg });

			StringToIndex = (arg)
				=> (int)obj.Invoke("StringToIndex", new object[1] { arg });

			_dedicatedAddresses = ()
				=> (Dictionary<string, int>)obj.GetFieldOrProperty("_dedicatedAddresses");
		}


		[Fact]
		public void CanEnumerateAddresses()
		{
			var address1 = IndexToString(0); // first value
			var address255 = IndexToString(255); // one byte is full
			var address255_p1 = IndexToString(255 + 1); // one byte is full + one started
			var address256_256_m1 = IndexToString(256 * 256 - 1); // two bytes are full
			var address256_256 = IndexToString(256 * 256); // two bytes are full + one started
			var address256_256_122_m1 = IndexToString(((128 - 6) * 256 * 256) + (256 * 256 - 1));

			Assert.Equal("10.6.0.0/32", address1);
			Assert.Equal("10.6.0.255/32", address255);
			Assert.Equal("10.6.1.0/32", address255_p1);
			Assert.Equal("10.6.255.255/32", address256_256_m1);
			Assert.Equal("10.7.0.0/32", address256_256);
			Assert.Equal("10.128.255.255/32", address256_256_122_m1);
		}

		[Fact]
		public void CanRegisterKey()
		{
			var key1 = Convert.ToBase64String(RandomNumberGenerator.GetBytes(256 / 8));
			var key2 = Convert.ToBase64String(RandomNumberGenerator.GetBytes(256 / 8));

			var ip1 = service.EnsureDedicatedAddressForPeer(key1);
			var ip2 = service.EnsureDedicatedAddressForPeer(key2);

			var ip11 = service.EnsureDedicatedAddressForPeer(key1);
			var ip22 = service.EnsureDedicatedAddressForPeer(key2);

			Assert.Equal(ip1, ip11);
			Assert.Equal(ip2, ip22);
		}

		[Fact]
		public void CanDeleteKey()
		{
			var key1 = Convert.ToBase64String(RandomNumberGenerator.GetBytes(256 / 8));
			var key2 = Convert.ToBase64String(RandomNumberGenerator.GetBytes(256 / 8));
			var key3 = Convert.ToBase64String(RandomNumberGenerator.GetBytes(256 / 8));

			/* supossed steps:
			 * 
			 * state 1:
			 * key 1 - 10.0.0.0
			 * key 2 - 10.0.0.1
			 * 
			 * state 2:
			 * key2 - 10.0.0.1
			 *
			 * state 3:
			 * key3 - 10.0.0.0
			 * key2 - 10.0.0.1
			 * 
			 * state 4:
			 * key3 - 10.0.0.0
			 * key2 - 10.0.0.1
			 * key1 - 10.0.0.2
			 */

			var ip1 = service.EnsureDedicatedAddressForPeer(key1);
			var ip2 = service.EnsureDedicatedAddressForPeer(key2);

			var del1 = service.DeletePeer(key1);
			Assert.True(del1);

			var del11 = service.DeletePeer(key1);
			Assert.False(del11);

			var ip3 = service.EnsureDedicatedAddressForPeer(key3);
			Assert.Equal(ip1, ip3);

			ip1 = service.EnsureDedicatedAddressForPeer(key1);
			Assert.StartsWith("10.6.0.0", ip3);
			Assert.StartsWith("10.6.0.1", ip2);
			Assert.StartsWith("10.6.0.2", ip1);
		}

		[Fact]
		public void CanParseAddress()
		{
			int[] Ids = new int[]
			{
				0,
				255,
				255 + 1,
				256 * 256 - 1,
				256 * 256,
				256 * 256 * 256 - 1,
				256 * 256 * 256
			};

			var converted = Ids.Select(x => IndexToString(x)).Select(x => StringToIndex(x));

			Assert.True(Ids.SequenceEqual(converted));
		}

		[Fact]
		public void CanSyncState()
		{
			var output = "interface: wg0\r\n  public key: Kq0LygX5ESfSpIDQO0k4dGSCnOAIZlJDPFacBeOBMCE=\r\n  private key: (hidden)\r\n  listening port: 51820\r\n\r\npeer: zO2jr59gppRHrPlaVshrm/AH8YUzVGToGDvoeS8j4CI=\r\n  endpoint: 31.173.84.131:21260\r\n  allowed ips: 10.8.0.101/32\r\n  latest handshake: 9 hours, 18 minutes, 25 seconds ago\r\n  transfer: 20.19 MiB received, 683.31 MiB sent\r\n  persistent keepalive: every 25 seconds\r\n\r\npeer: 3Sg1yyugDmxoYFnEu+1i280QZpgD8tAHHHXUZPWDSk4=\r\n  endpoint: 5.188.98.224:60796\r\n  allowed ips: 10.8.0.115/32\r\n  latest handshake: 1 day, 7 hours, 7 minutes, 11 seconds ago\r\n  transfer: 116.43 MiB received, 2.32 GiB sent\r\n  persistent keepalive: every 25 seconds\r\n\r\npeer: MTy1q7mVlyuUkCnvz0ZrXPCTSzhbjNMbhzfJU/gSsF8=\r\n  endpoint: 31.173.82.117:25566\r\n  allowed ips: 10.8.0.100/32\r\n  latest handshake: 1 day, 13 hours, 13 minutes, 53 seconds ago\r\n  transfer: 13.19 MiB received, 176.12 MiB sent\r\n  persistent keepalive: every 25 seconds\r\n\r\npeer: V1evhjZQhhygsdrtriAb5AzuUuE8SkQNHJ4YAYdxGQs=\r\n  allowed ips: 10.0.0.0/32\r\n  persistent keepalive: every 25 seconds\r\n\r\npeer: HTqjO7TgQ1Mke2PKPtC2XGrAAK1INyH6j9ke7cn8cQU=\r\n  allowed ips: 10.1.1.1/32\r\n  persistent keepalive: every 25 seconds\r\n\r\npeer: LwkXubXSXLOzQPK0a6PQp1DWz08lsfk+Oyp7s1056H8=\r\n  allowed ips: 10.6.6.6/32\r\n  persistent keepalive: every 25 seconds\r\n\r\npeer: 9c0dwlfFnTPuVon4au2l3mx94jme2czT4CSkd8ZbODM=\r\n  allowed ips: 10.64.64.64/32\r\n  persistent keepalive: every 25 seconds\r\n\r\npeer: /T3Yzw1oFJYYLDsC2bVG1UE2q2fuUPppSI+O3tr18Ek=\r\n  allowed ips: 10.255.255.255/32\r\n  persistent keepalive: every 25 seconds\r\n\r\npeer: rLb4WX7XDfOIs69hO1CGUrQuUqn42NT7OFAnttnupGA=\r\n  allowed ips: 10.8.0.102/32\r\n  persistent keepalive: every 25 seconds\r\n\r\npeer: hSDPy+JWh+IYRK72AThfvET+N8B8/i7SBWvTq1m8Y1I=\r\n  allowed ips: 10.8.0.110/32\r\n  persistent keepalive: every 25 seconds";
			var peers = WgStatusParser.ParsePeersFromWgShow(output, out _);

			foreach (var p in peers)
			{
				service.EnsureDedicatedAddressForPeer(p.PublicKey);
			}

			const int left1 = 1;
			const int left2 = 3;

			var remain = new WgFullPeerInfo[]
				{peers[left1],peers[left2]};

			service.SyncState(remain.ToDictionary(x => x.PublicKey));

			var actuallyRemain = _dedicatedAddresses();

			Assert.Equal(2, actuallyRemain.Count);
			Assert.Equal(remain[0].AllowedIps, service.EnsureDedicatedAddressForPeer(remain[0].PublicKey));
			Assert.Equal(remain[1].AllowedIps, service.EnsureDedicatedAddressForPeer(remain[1].PublicKey));

			for (int i = 0; i < peers.Count; i++)
			{
				if (i == left1 || i == left2)
				{
					Assert.True(service.DeletePeer(peers[i].PublicKey));
					_output.WriteLine("Asserted true.");
				}
				else
				{
					Assert.False(service.DeletePeer(peers[i].PublicKey));
					_output.WriteLine("Asserted false.");
				}
			}
		}
	}
}
