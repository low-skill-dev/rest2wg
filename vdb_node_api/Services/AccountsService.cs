using IdentityModel;
using System.Security.Cryptography;

namespace vdb_node_api.Services
{
	/* Singleton-сервис, отвечающий за проверку аккаунтов.
	 */
	public sealed class AccountsService
	{
		private class AccountInfo
		{
			public byte[] KeyHash;
			public byte[] JwtRefreshKey;

			public AccountInfo(byte[] keyHash, byte[] jwtRefreshKey)
			{
				this.KeyHash = keyHash;
				this.JwtRefreshKey = jwtRefreshKey;
			}
		}

		private readonly List<AccountInfo > _accountsKeyHashes;

		public AccountsService(SettingsProviderService settingsProvider) 
		{ 
			_accountsKeyHashes = settingsProvider.MasterAccounts.Select(x => new AccountInfo(Base64Url.Decode(x.KeyHashBase64), null!)).ToList();
		}

		public bool IsApiKeyValid(string keyBase64)
		{
			var hashed = SHA512.HashData(Base64Url.Decode(keyBase64));

			return _accountsKeyHashes.Any(x=> x.KeyHash.SequenceEqual(hashed));
		}

		public string? IsRefreshKeyValid(string keyBase64)
		{
			var hashed = SHA512.HashData(Base64Url.Decode(keyBase64));

			var found = _accountsKeyHashes.FirstOrDefault(x=>  x.KeyHash.SequenceEqual(hashed));

			if(found is null || found.JwtRefreshKey is null)
			{
				return null;
			}

			return Base64Url.Encode(found.JwtRefreshKey.SequenceEqual()
		}
	}
}
