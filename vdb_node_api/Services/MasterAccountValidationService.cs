using AutoMapper.Internal;
using Duende.IdentityServer.Models;
using IdentityModel;
using System.Runtime;
using System.Security.Cryptography;
using vdb_node_api.Models.Runtime;

namespace vdb_node_api.Services
{
	/* Singleton-сервис, обеспечивающий валидацию мастер-аккаунтов.
	 * Данный сервис хранит хеши валидных ключей в виде HashSet<string> в ОЗУ,
	 * поэтому общее количество мастер-аккаунтов должно быть ограничено,
	 * например, одной тысячей. Длинна Api-ключа, обычно, составляет 1024 байта.
	 */
	public sealed class MasterAccountService
	{
		/* Мастер-аккаунты могут добавлять динамически и функционировать
		 * до рестарта приложения.
		 */
		public class DynamicAccountInfo
		{
			public DateTime CreatedUtc { get; set; }
			public DateTime LastAccessUtc { get; set; }
			public TimeSpan LifeSpanAfterLastAccess { get; set; }
			public DateTime ValidUntilUtc { get; set; }

			// Общий срок жизни не истек && со времени последнего доступа прошло меньше лайфспана
			public bool IsValid =>
				DateTime.UtcNow < ValidUntilUtc && (DateTime.UtcNow - LastAccessUtc) < LifeSpanAfterLastAccess;

			private DynamicAccountInfo(
				DateTime createdUtc, DateTime lastAccessedUtc, 
				DateTime validUntilUtc, TimeSpan lifeSpanAfterLastAccess)
			{
				this.CreatedUtc = createdUtc;
				this.LastAccessUtc = lastAccessedUtc;
				this.LifeSpanAfterLastAccess = lifeSpanAfterLastAccess;
				this.ValidUntilUtc = validUntilUtc;
			}

			public DynamicAccountInfo(DateTime validUntilUtc, TimeSpan lifeSpanAfterLastAccess)
				: this(DateTime.UtcNow, DateTime.UtcNow, validUntilUtc, lifeSpanAfterLastAccess)
			{

			}
		}

		private readonly SettingsProviderService _settingsProvider;
		private MasterAccountServiceSettings _settings => _settingsProvider.MasterAccountServiceSettings;
		private readonly ILogger<MasterAccountService> _logger;

		// HashSet гарантирует уникальность в т.ч. и ссылочных типов, наследуемых от IEquatable
		private HashSet<string> _accounts;
		private Dictionary<string, DynamicAccountInfo> _dynamicallyAddedAccounts;
		private DateTime _dynamicAccountsLastCompressedUtc;

		public MasterAccountService(SettingsProviderService settingsProvider, ILogger<MasterAccountService> logger)
		{
			// Инициализируем обязательные поля.
			_accounts = new();
			_dynamicallyAddedAccounts = new();
			_dynamicAccountsLastCompressedUtc = DateTime.UtcNow;

			// Разбираемся с зависимостями, переданными в конструктор.
			_settingsProvider = settingsProvider;
			_logger = logger;
			var providedAccounts = settingsProvider.MasterAccounts;

			// Число ключей превышает максимальное - лог критикал + исключение.
			if (providedAccounts.Length > _settings.MaxNumberOfMasterAccounts)
			{
				var errorMessage = $"Too many master accounts was declared in the server " +
					$"configuration: {providedAccounts.Length}, while the max number was: " +
					$"{_settings.MaxNumberOfMasterAccounts}.";

				_logger.LogCritical(errorMessage);
				throw new ArgumentOutOfRangeException(errorMessage);
			}

			// Создаем коллекцию нужного размера
			_accounts.EnsureCapacity(providedAccounts.Length);

			//Добавляем все ключи мастер-аккаунтов.
			for (int i = 0; i < providedAccounts.Length; i++)
			{
				// Обнаружен дубликат ключа
				if (!_accounts.Add(providedAccounts[i].KeyHashBase64))
				{
					var errorMessage = $"There was a duplicate of key hash: " +
						$"{providedAccounts[i].KeyHashBase64}.";

					// Дубликаты разрешены - лог предупреждение.
					if (_settings.IgnoreKeyDuplicates)
					{
						_logger.LogWarning(errorMessage);
					}
					// Дубликаты запрещены - лог критикал + исключение.
					else
					{
						_logger.LogCritical(errorMessage);
						throw new ArgumentException(errorMessage);
					}
				}
				else
				{
					_logger.LogInformation($"Successfully added master-account key hash: " +
						$"{providedAccounts[i].KeyHashBase64}.");
				}

			}
		}

		private string KeyToHashString(string base64UrlKey)
		{
			return Base64Url.Encode(KeyToHashBytes(base64UrlKey));
		}
		private byte[] KeyToHashBytes(string base64UrlKey)
		{
			return SHA512.HashData(Base64Url.Decode(base64UrlKey));
		}

		private bool IsMasterValid(string hash)
		{
			return this._accounts.Contains(hash);
		}
		private bool IsDynamicValid(string hash)
		{
			if(this._dynamicallyAddedAccounts.TryGetValue(hash, out var account))
			{
				if (account.IsValid)
				{
					return true;
				}
				else
				{
					this._dynamicallyAddedAccounts.Remove(hash);
				}
			}

			return false;
		}

		/// <returns>true if the key is valid for operations, false otherwise.</returns>
		public bool IsValid(string base64UrlKey)
		{
			var hash = KeyToHashString(base64UrlKey);
			return IsMasterValid(hash) || IsDynamicValid(hash);
		}

		/// <returns>true if the key was successfully invalidated, false if was not valid already.</returns>
		public bool InvalidateSelf(string base64UrlKey)
		{
			var hash = KeyToHashString(base64UrlKey);
			return this._accounts.Remove(hash) || this._dynamicallyAddedAccounts.Remove(hash);
		}

		/// <returns>true if all keys was successfully invalidated, false otherwise.</returns>
		public bool InvalidateAll(string base64UrlKey)
		{
			var hash = KeyToHashString(base64UrlKey);

			// Только исходный мастер-аккаунт может выполнить общую инвалидизацию
			if (IsMasterValid(hash))
			{
				this._accounts.Clear();
				this._dynamicallyAddedAccounts.Clear();
				return true;
			}
			else
			{
				return false;
			}
		}

		/// <returns>true if the key was successfully invalidated, false otherwise.</returns>
		public bool InvalidateDynamicByMaster(string base64UrlMasterKey, string base64UrlDynamicKey)
		{
			var dynamicHash = KeyToHashString(base64UrlDynamicKey);
			return InvalidateDynamicHashByMaster(base64UrlMasterKey, dynamicHash);
		}
		/// <returns>true if the key was successfully invalidated, false otherwise.</returns>
		public bool InvalidateDynamicHashByMaster(string base64UrlMasterKey, string base64UrlDynamicHash)
		{
			var masterHash = KeyToHashString(base64UrlMasterKey);
			if (IsMasterValid(masterHash))
			{
				return this._dynamicallyAddedAccounts.Remove(base64UrlDynamicHash);
			}
			else
			{
				return false;
			}
		}

		/// <returns>true if key was created and successfully added, false otherwise.</returns>
		public bool CreateKeyDynamically(out string? createdKeyBase64Url)
		{
			return CreateKeyDynamicallyPrivate(null, out createdKeyBase64Url);
		}

		/// <returns>true if key was created and successfully added, false otherwise.</returns>
		public bool CreateKeyDynamically(DynamicAccountInfo accountInfo, out string? createdKeyBase64Url)
		{
			return CreateKeyDynamicallyPrivate(accountInfo, out createdKeyBase64Url);
		}

		// Создает новый динамический ключ и добавляет его хеш в коллекцию.
		private bool CreateKeyDynamicallyPrivate(DynamicAccountInfo? accountInfo, out string? createdKeyBase64Url)
		{
			createdKeyBase64Url = null;

			// Если прошел интервал сжатия колекции - выполнить сжатие
			if (DateTime.UtcNow - this._dynamicAccountsLastCompressedUtc 
				> _settings.DynamicAccountsCollectionCompressionMinInterval)
			{
				this.CompressDynamicAccountsCollection();
			}
			// Если достигнуто максимальное число динамических аккаунтов - отказать в создании
			if (_dynamicallyAddedAccounts.Count > _settings.MaxNumberOfDynamicMasterAccounts)
			{
				return false;
			}

			// Если не передано информации о новом ключе - использовать настройки
			if (accountInfo is null) accountInfo = new(
				DateTime.UtcNow.Add(_settings.MaxLifeSpanOfDynamicMasterAccount),
				_settings.MaxLifeSpanAfterLastAccessOfDynamicMasterAccount);

			// Есть почти невероятный шанс, что будет сгенерирован дубликат,
			// не стоит это предусматривать.
			var generatedKey = RandomNumberGenerator.GetBytes(_settings.KeyLengthBytes);
			var generatedHash = SHA512.HashData(generatedKey);

			var keyBase64 = Base64Url.Encode(generatedKey);
			var hashBase64 = Base64Url.Encode(generatedHash);

			// добавляем хеш
			_dynamicallyAddedAccounts.Add(hashBase64, accountInfo);

			// отдаем ключ
			createdKeyBase64Url = keyBase64;
			return true;
		}

		// Удаляет устаревшие динамические ключи из коллекции
		private void CompressDynamicAccountsCollection()
		{
			var keys = this._dynamicallyAddedAccounts.Keys;
			foreach (var key in keys)
			{
				if (!this._dynamicallyAddedAccounts[key].IsValid)
				{
					this._dynamicallyAddedAccounts.Remove(key);
				}
			}
		}
	}
}
