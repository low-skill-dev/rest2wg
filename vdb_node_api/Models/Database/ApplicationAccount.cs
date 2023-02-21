using Microsoft.EntityFrameworkCore;

namespace vdb_node_api.Models.Database
{
	/* Данный класс хранит аккаунт приложения (а в действительности - любого HTTP клиента), 
	 * которое может управлять конфигурацией Wireguard-сервиса, путем обращения к API.
	 * 
	 * Каждое приложение первично проходит аутентификацию по своему API-ключу, для которого
	 * вычисляется хеш и сравнивается с хранимым в базе данных.
	 * 
	 * Для каждого приложения может задаваться имя, служащее больше инструментом администрирования.
	 * 
	 * Каждое приложение, по задумке, может обновлять собственной токен, для этого предусмотрены
	 * поля RefreshNotBeforeUtc и RefreshNotAfterUtc.
	 * 
	 * Свойство IsPendingDeletion указывает, что аккаунт не может быть как-либо использован.
	 * 
	 * Класс заполнен стандартными зачениями. Их настройка через appsettings.json не является
	 * критической (в отличии, например, от ключей подписи, при отсутствии которых должно
	 * выбрасываться исключение). Однако, с высокой вероятностью, все эти значения будут
	 * переопределяться соответствующим сервисом.
	 * 
	 * Id является PK, ApiKeyHash является AK. (Primary/Alternate keys).
	 */
	public class ApplicationAccount
	{
		public int Id { get; set; }
		public string ApiKeyHash { get; set; } // byte[] creates migration problems
		public string[] RefreshJwtKeysHashes { get; set; }
		public DateTime LastAccessedUtc { get; set; }
	

		public ApplicationAccount(string apiKeyHash)
		{
			this.ApiKeyHash = apiKeyHash;
			this.LastAccessedUtc = DateTime.UtcNow;
		}
	}
}
