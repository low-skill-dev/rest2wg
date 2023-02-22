using System.Security.Cryptography;
using vdb_node_api.Models;

namespace vdb_node_api.Services
{
	/* Singleton-сервис для валидации мастер-аккаунтов.
	 */
	public sealed class MasterAccountsService
	{
		private readonly List<byte[]> _mastersKeyHashes;
		public MasterAccountsService(SettingsProviderService settingsProvider)
		{
			_mastersKeyHashes = settingsProvider.MasterAccounts
				.Select(x => Convert.FromBase64String(x.KeyHashBase64)).ToList();
		}

		/// <returns>true if passed key is valid, false otherwise.</returns>
		public bool IsValid(string keyBase64)
		{
			var search = SHA512.HashData(Convert.FromBase64String(keyBase64));
			return _mastersKeyHashes.Any(k => k.SequenceEqual(search));
		}

		/// <returns>true if passed key was invalidated, false if was not valid already.</returns>
		public bool Invalidate(string keyBase64)
		{
			var search = SHA512.HashData(Convert.FromBase64String(keyBase64));
			var index = _mastersKeyHashes.FindIndex(k => k.SequenceEqual(search));
			if (index < 0) return false;

			_mastersKeyHashes.RemoveAt(index);
			return true;
		}
	}
}
