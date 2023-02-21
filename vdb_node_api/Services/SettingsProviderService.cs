using vdb_node_api.Models.Runtime;
using Microsoft.Extensions.Configuration;

namespace vdb_node_api.Services
{
	/* Класс служит для повышения уровня абстракции в других сервисах.
	 * Обеспечивает получение настроек из appsettings и прочих файлов
	 * с последующей их записью в соответствующие модели.
	 */
	public class SettingsProviderService
	{
		protected IConfiguration _configuration;


		public virtual MasterAccount[] MasterAccounts
			=> _configuration.GetSection(nameof(MasterAccounts)).Get<MasterAccount[]>() ?? Array.Empty<MasterAccount>();
		public virtual MasterAccountServiceSettings MasterAccountServiceSettings
			=> _configuration.GetSection(nameof(MasterAccountServiceSettings)).Get<MasterAccountServiceSettings>() ?? new();

		public virtual JwtServiceSettings JwtServiceSettings
			=> _configuration.GetSection(nameof(JwtServiceSettings)).Get<JwtServiceSettings>() ?? new();

		public virtual ApplicationAccountServiceSettings ApplicationAccountServiceSettings
			=> _configuration.GetSection(nameof(ApplicationAccountServiceSettings)).Get<ApplicationAccountServiceSettings>() ?? new();


		public SettingsProviderService(IConfiguration configuration)
		{
			this._configuration = configuration;
		}
	}
}
