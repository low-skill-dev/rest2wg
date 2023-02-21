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
		public long Id { get; set; }
		public string? Name { get; set; }
		public string ApiKeyHash { get; set; } // used as password, could use byte[], but it creates migration problems

		public DateTime CreatedDateTimeUtc { get; set; } = DateTime.UtcNow;
		public DateTime LastAccessDateTimeUtc { get; set; } = DateTime.UtcNow;

		public DateTime AccessNotBeforeUtc { get; set; } = DateTime.UtcNow;
		public DateTime AccessNotAfterUtc { get; set; } = DateTime.UtcNow.AddDays(1);
		public bool CanAccessNow => DateTime.UtcNow > AccessNotBeforeUtc && DateTime.UtcNow < AccessNotAfterUtc;

		public DateTime RefreshNotBeforeUtc { get; set; } = DateTime.UtcNow.AddMinutes(60);
		public DateTime RefreshNotAfterUtc { get; set; } = DateTime.UtcNow.AddDays(7);
		public bool CanRefreshNow => DateTime.UtcNow > RefreshNotBeforeUtc && DateTime.UtcNow < RefreshNotAfterUtc;

		public bool IsPendingDeletion => !(CanAccessNow || CanRefreshNow);

		public ApplicationAccount(string apiKeyHash)
		{
			this.ApiKeyHash = apiKeyHash;
		}
	}
}
