using Microsoft.Extensions.Logging.Abstractions;
using Moq;
using System.Security.Cryptography;
using vdb_node_api.Services;
using Xunit.Abstractions;

namespace vdb_node_api.tests.Services;

public class MasterAccountsServiceTests
{
	private readonly MasterAccountsService service;
	private readonly SettingsProviderService settingsProvider;
	private readonly byte[][] keys;
	public MasterAccountsServiceTests(ITestOutputHelper output)
	{
		keys = new byte[100].Select(x => RandomNumberGenerator.GetBytes(512 / 8)).ToArray();

		Mock<SettingsProviderService> spMock = new(null,
			new EnvironmentProvider(new NullLogger<EnvironmentProvider>()));
		spMock.SetupGet(x => x.MasterAccounts).Returns(keys
			.Select(x => Convert.ToBase64String(SHA512.HashData(x)))
			.Select(x => new Models.Runtime.MasterAccount(x)).ToArray());

		settingsProvider = spMock.Object;
		service = new(settingsProvider);
	}

	[Fact]
	public void CanValidateKey()
	{
		foreach (byte[] k in keys)
		{
			Assert.True(service.IsValid(Convert.ToBase64String(k)));
		}
	}

	[Fact]
	public void CanRejectKey()
	{

		var randomKeys = new byte[100].Select(x => RandomNumberGenerator.GetBytes(512 / 8));

		foreach (byte[]? k in randomKeys)
		{
			Assert.False(service.IsValid(Convert.ToBase64String(k)));
		}
	}
}
