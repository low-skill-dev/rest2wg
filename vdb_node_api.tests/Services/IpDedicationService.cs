using System.Security.Cryptography;
using vdb_node_api.Services;

namespace vdb_node_api.tests.Services
{
	public class IpDedicationServiceTests
	{
		private IpDedicationService service;
		private Func<string, int> StringToIndex;
		private Func<int, string> IndexToString;
		public IpDedicationServiceTests()
		{
			service = new();
			var obj = new Microsoft.VisualStudio.TestTools.UnitTesting.PrivateObject(service);

			IndexToString = (arg)
				=> (string)obj.Invoke("IndexToString", new object[1] { arg });

			StringToIndex = (arg)
				=> (int)obj.Invoke("StringToIndex", new object[1] { arg });
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
	}
}
