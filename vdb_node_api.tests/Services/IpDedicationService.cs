using System.Security.Cryptography;
using vdb_node_api.Services;
using vdb_node_wireguard_manipulator;
using Xunit.Abstractions;

namespace vdb_node_api.tests.Services;

public class IpDedicationServiceTests
{
	private readonly ITestOutputHelper _output;
	private readonly IpDedicationService service;
	private readonly Func<string, int> StringToIndex;
	private readonly Func<int, string> IndexToString;
	private readonly Func<Dictionary<string, int>> _dedicatedAddresses;
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
		string address1 = IndexToString(0); // first value
		string address255 = IndexToString(255); // one byte is full
		string address255_p1 = IndexToString(255 + 1); // one byte is full + one started
		string address256_256_m1 = IndexToString((256 * 256) - 1); // two bytes are full
		string address256_256 = IndexToString(256 * 256); // two bytes are full + one started
		string address256_256_122_m1 = IndexToString(((128 - 6) * 256 * 256) + ((256 * 256) - 1));

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
		string key1 = Convert.ToBase64String(RandomNumberGenerator.GetBytes(256 / 8));
		string key2 = Convert.ToBase64String(RandomNumberGenerator.GetBytes(256 / 8));

		string ip1 = service.EnsureDedicatedAddressForPeer(key1);
		string ip2 = service.EnsureDedicatedAddressForPeer(key2);

		string ip11 = service.EnsureDedicatedAddressForPeer(key1);
		string ip22 = service.EnsureDedicatedAddressForPeer(key2);

		Assert.Equal(ip1, ip11);
		Assert.Equal(ip2, ip22);
	}

	[Fact]
	public void CanDeleteKey()
	{
		string key1 = Convert.ToBase64String(RandomNumberGenerator.GetBytes(256 / 8));
		string key2 = Convert.ToBase64String(RandomNumberGenerator.GetBytes(256 / 8));
		string key3 = Convert.ToBase64String(RandomNumberGenerator.GetBytes(256 / 8));

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

		string ip1 = service.EnsureDedicatedAddressForPeer(key1);
		string ip2 = service.EnsureDedicatedAddressForPeer(key2);

		bool del1 = service.DeletePeer(key1);
		Assert.True(del1);

		bool del11 = service.DeletePeer(key1);
		Assert.False(del11);

		string ip3 = service.EnsureDedicatedAddressForPeer(key3);
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
			(256 * 256) - 1,
			256 * 256,
			(256 * 256 * 256) - 1,
			256 * 256 * 256
		};

		var converted = Ids.Select(x => IndexToString(x)).Select(x => StringToIndex(x));

		Assert.True(Ids.SequenceEqual(converted));
	}
}
