namespace vdb_node_api.Models.Api.Application
{

	/* Некоторые приложения могут порождать других пользователей приложения.
	 * Сами таковые приложения должны регистрировать через файл секретов при развертывании.
	 * Их динамическое создание во время исполнения невозможно в целях безопасности, однако
	 * возможно их временная блокировка путем отправки соответствующего запроса.
	 */
	public class RegistrationRequest
	{
		public string MasterApiKey { get; set; }

		public RegistrationRequest(string masterApiKey)
		{
			this.MasterApiKey = masterApiKey;
		}
	}
}
