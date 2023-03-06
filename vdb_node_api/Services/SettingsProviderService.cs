using Microsoft.Extensions.Configuration;
using vdb_node_api.Models;
using vdb_node_api.Models.ServicesSettings;

namespace vdb_node_api.Services;

/* Sigleton-сервис, служит для повышения уровня абстракции в других сервисах.
 * Обеспечивает получение настроек из appsettings и прочих файлов
 * с последующей их записью в соответствующие модели.
 */
public class SettingsProviderService
{
	protected IConfiguration _configuration;

	public virtual MasterAccount[] MasterAccounts =>
		_configuration.GetSection(nameof(MasterAccounts)).Get<MasterAccount[]>()
		?? Array.Empty<MasterAccount>();

	public virtual PeersBackgroundServiceSettings PeersBackgroundServiceSettings =>
		_configuration.GetSection(nameof(PeersBackgroundServiceSettings))
		.Get<PeersBackgroundServiceSettings>()
		?? new PeersBackgroundServiceSettings(3600,3600);



	public SettingsProviderService(IConfiguration configuration)
	{
		this._configuration = configuration;
	}
}

