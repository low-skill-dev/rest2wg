using IdentityModel;
using Microsoft.EntityFrameworkCore.ChangeTracking;
using Newtonsoft.Json.Linq;
using System.Data.Entity.Core.Common.CommandTrees.ExpressionBuilder;
using System.Runtime.InteropServices;
using System.Security.Cryptography;
using vdb_node_api.Controllers;
using vdb_node_api.Infrastructure.Database;
using vdb_node_api.Models.Database;
using vdb_node_api.Models.Runtime;
using Z.EntityFramework.Plus;

namespace vdb_node_api.Services
{
	/* Singleton-сервис, отвечающий за генерацию новых аккаунтов,
	 * очистку устаревших.
	 */
	public class ApplicationAccountService
	{
		protected readonly SettingsProviderService _settingsProvider;
		protected virtual ApplicationAccountServiceSettings _settings => _settingsProvider.ApplicationAccountServiceSettings;
		private readonly VdbNodeContext _context;
		private readonly ILogger<ApplicationAccountService> _logger;
		private DateTime _lastAccountsDeletedUtc;

		public ApplicationAccountService(VdbNodeContext vdbNodeContext, SettingsProviderService settingsProvider, ILogger<ApplicationAccountService> logger)
		{
			_context = vdbNodeContext;
			_settingsProvider = settingsProvider;
			_logger = logger;
		}

		private byte[] GenerateKey(int? lengthBytes = null)
		{
			return RandomNumberGenerator.GetBytes(lengthBytes ?? _settings.KeyLengthBytes);
		}
		public string GenerateKeyAndHashBase64Url(out string hashBase64Url, int? lengthBytes = null)
		{
			var key = GenerateKey(lengthBytes);
			var hash = SHA512.HashData(key);

			hashBase64Url = Base64Url.Encode(hash);
			return Base64Url.Encode(key);
		}

		private string GenerateRandomName()
		{
			return string.Join(' ', BitConverter.GetBytes(DateTime.UtcNow.Ticks).Select(x => $"{x:000}")); ;
		}

		/// <returns>
		/// Newly created key and account generated for it as a Tuple of string (key)
		/// and EntityEntry (generated account, added to Db).
		/// </returns>
		public async Task<Tuple<string, EntityEntry<ApplicationAccount>>> CreateNewAccount(string? name = null)
		{
			if ((DateTime.UtcNow - _lastAccountsDeletedUtc).TotalSeconds > _settings.AccountsDeletionIntervalSeconds)
			{
				await DeleteInvalidAccounts();
			}
			if (_context.ApplicationAccounts.Count() > _settings.MaxNumberOfApplicationAccounts)
			{
				throw new OutOfMemoryException("There was maximum number of accounts " +
					"already exist in the database.");
			}


			if (name is null && _settings.GenerateRandomNameForAccounts)
			{
				name = GenerateRandomName();
			}

			var generatedKey = GenerateKeyAndHashBase64Url(out var generatedHash);

			var now = DateTime.UtcNow;
			var generatedAccount = new ApplicationAccount(generatedHash)
			{
				Name = name,
				CreatedDateTimeUtc = now,
				LastAccessDateTimeUtc = now,
				AccessNotBeforeUtc = now.AddSeconds(_settings.AccessNotBeforeFromUtcNowSeconds),
				AccessNotAfterUtc = now.AddSeconds(_settings.AccessNotAfterFromUtcNowSeconds),
				RefreshNotBeforeUtc = now.AddSeconds(_settings.RefreshNotBeforeFromUtcNowSeconds),
				RefreshNotAfterUtc = now.AddSeconds(_settings.RefreshNotAfterFromUtcNowSeconds)
			};

			return new(generatedKey, await _context.ApplicationAccounts.AddAsync(generatedAccount));
		}

		/// <returns>the new key inserted into entity without saving its changes to any database.</returns>
		public string RefreshAccountEntityKey(EntityEntry<ApplicationAccount> entityEntry)
		{
			var newKey = GenerateKeyAndHashBase64Url(out var newHash);

			var entity = entityEntry.Entity;

			var now = DateTime.UtcNow;
			entity.LastAccessDateTimeUtc = now;
			entity.AccessNotBeforeUtc = now.AddSeconds(_settings.AccessNotBeforeFromUtcNowSeconds);
			entity.AccessNotAfterUtc = now.AddSeconds(_settings.AccessNotAfterFromUtcNowSeconds);
			entity.RefreshNotBeforeUtc = now.AddSeconds(_settings.RefreshNotBeforeFromUtcNowSeconds);
			entity.RefreshNotAfterUtc = now.AddSeconds(_settings.RefreshNotAfterFromUtcNowSeconds);

			entity.ApiKeyHash = newHash;

			return newKey;
		}

		/// <returns>the number of rows deleted.</returns>
		private async Task<int> DeleteInvalidAccounts()
		{
			return await _context.ApplicationAccounts.Where(acc =>
				acc.AccessNotAfterUtc < DateTime.UtcNow &&
				acc.RefreshNotAfterUtc < DateTime.UtcNow).DeleteAsync();
		}
	}
}
