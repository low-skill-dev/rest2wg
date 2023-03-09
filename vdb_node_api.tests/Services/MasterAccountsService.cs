using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;
using Moq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;
using vdb_node_api.Services;
using Xunit.Abstractions;

namespace vdb_node_api.tests.Services
{
	public class MasterAccountsServiceTests
	{
		private MasterAccountsService service;
		private SettingsProviderService settingsProvider;
		private byte[][] keys;
		public MasterAccountsServiceTests(ITestOutputHelper output)
		{
			keys = new byte[100].Select(x => RandomNumberGenerator.GetBytes(512 / 8)).ToArray();

			Mock<SettingsProviderService> spMock = new(null,
				new EnvironmentProvider(new NullLogger<EnvironmentProvider>()));
			spMock.SetupGet(x => x.MasterAccounts).Returns(keys
				.Select(x => Convert.ToBase64String(SHA512.HashData(x)))
				.Select(x => new Models.MasterAccount(x)).ToArray());

			settingsProvider = spMock.Object;
			service = new(settingsProvider);
		}

		[Fact]
		public void CanValidateKey()
		{
			foreach(var k in keys)
			{
				Assert.True(service.IsValid(Convert.ToBase64String(k)));
			}
		}

		[Fact]
		public void CanRejectKey()
		{

			var randomKeys = new byte[100].Select(x => RandomNumberGenerator.GetBytes(512 / 8));

			foreach (var k in randomKeys)
			{
				Assert.False(service.IsValid(Convert.ToBase64String(k)));
			}
		}

		//[Fact]
		[Obsolete]
		public void CanInvalidateKey()
		{
			foreach( var k in keys)
			{
				var keyStr = Convert.ToBase64String(k);

				Assert.True(service.IsValid(keyStr));

				service.Invalidate(keyStr);

				Assert.False(service.IsValid(keyStr));
			}
		}
	}
}
