using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Security.Cryptography;
using vdb_node_api.Services;

namespace vdb_node_api.tests.Services
{
	//public class IpDedicationServiceExposed :
	public class IpDedicationServiceTests
	{
		private IpDedicationService service;
		private Func<int, byte[]> IndexToAddress;
		private Func<string, byte[]> AddressToBytes;
		private Func<byte[], string> BytesToAddress;
		public IpDedicationServiceTests()
		{
			service = new();
			var obj = new Microsoft.VisualStudio.TestTools.UnitTesting.PrivateObject(service);

			IndexToAddress = (arg)
				=> (byte[])obj.Invoke("IndexToAddress", new object[1] { arg });

			AddressToBytes = (arg)
				=> (byte[])obj.Invoke("AddressToBytes", new object[1] { arg });

			BytesToAddress = (arg)
				=> (string)obj.Invoke("BytesToAddress", new object[1] { arg });
		}


		[Fact]
		public void CanEnumerateAddresses()
		{
			var address1 = IndexToAddress(0);
			var address254 = IndexToAddress(254);
			var address255 = IndexToAddress(255);
			var address255_255 = IndexToAddress(255 * 255);

			Assert.Equal(new byte[] { 10, 1, 1, 1 }, address1);
			Assert.Equal(new byte[] { 10, 1, 1, 255 }, address254);
			Assert.Equal(new byte[] { 10, 1, 2, 1 }, address255);
			Assert.Equal(new byte[] { 10, 2, 1, 1 }, address255_255);

			var address255_255_p128 = IndexToAddress(255 * 255 + 128);
			var address255_255_254 = IndexToAddress(255 * 255 * 254);
			var address255_255_255_m1 = IndexToAddress(255 * 255 * 255 - 1);

			Assert.Equal(new byte[] { 10, 2, 1, 1 + 128 }, address255_255_p128);
			Assert.Equal(new byte[] { 10, 255, 1, 1 }, address255_255_254);
			Assert.Equal(new byte[] { 10, 255, 255, 255 }, address255_255_255_m1);
		}

		[Fact]
		public void CanRegisterKey()
		{
			var key1 = Convert.ToBase64String(RandomNumberGenerator.GetBytes(256 / 8));
			var key2 = Convert.ToBase64String(RandomNumberGenerator.GetBytes(256 / 8));

			var ip1 = service.EnsureDedicatedAddressForPeer(key1);
			var ip2 = service.EnsureDedicatedAddressForPeer(key2);

			Assert.Equal("10.1.1.1/32", ip1);
			Assert.Equal("10.1.1.2/32", ip2);

			var ip11 = service.EnsureDedicatedAddressForPeer(key1);
			var ip22 = service.EnsureDedicatedAddressForPeer(key2);

			Assert.Equal(ip1, ip11);
			Assert.Equal(ip2, ip22);
		}

		[Fact]
		public void CanParseAddress()
		{
			var address = IndexToAddress(255 * 255 + 128);
			var asString = BytesToAddress(address);
			var asBytes = AddressToBytes(asString);

			Assert.Equal("10.2.1.129/32", asString);
			Assert.Equal(new byte[] { 10, 2, 1, 1 + 128 }, asBytes);
		}
	}
}
