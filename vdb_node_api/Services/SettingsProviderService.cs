using vdb_node_api.Models.Runtime;

namespace vdb_node_api.Services;

/* Sigleton-сервис, служит для повышения уровня абстракции в других сервисах.
 * Обеспечивает получение настроек из appsettings и прочих файлов
 * с последующей их записью в соответствующие модели.
 */
public class SettingsProviderService
{
	protected readonly IConfiguration _configuration;
	protected readonly EnvironmentProvider _environment;

	public virtual MasterAccount[] MasterAccounts
	{
		get
		{
			var fromConf = _configuration.GetSection(nameof(MasterAccounts)).Get<MasterAccount[]>();

			if (_environment.AUTH_KEYHASH is not null)
			{
				if (fromConf is not null)
				{
					var newArr = new MasterAccount[fromConf.Length];
					fromConf.CopyTo(newArr, 0);
					newArr[newArr.Length - 1] = new(_environment.AUTH_KEYHASH);
				}
				else
				{
					return new MasterAccount[1] { new(_environment.AUTH_KEYHASH) };
				}
			}
			return fromConf ?? Array.Empty<MasterAccount>();
		}
	}


	public virtual PeersBackgroundServiceSettings PeersBackgroundServiceSettings
	{
		get
		{
			var fromConf = _configuration.GetSection(nameof(PeersBackgroundServiceSettings))
				.Get<PeersBackgroundServiceSettings>()
				?? new PeersBackgroundServiceSettings(3600, 600);

			// overriding by env
			if (_environment.REVIEW_INTERVAL is not null)
				fromConf.PeersRenewIntervalSeconds = _environment.REVIEW_INTERVAL.Value;
			if (_environment.HANDSHAKE_AGO_LIMIT is not null)
				fromConf.HandshakeAgoLimitSeconds = _environment.HANDSHAKE_AGO_LIMIT.Value;

			return fromConf;
		}
	}

	public virtual SecretSigningKey? SecretSigningKey
		=> _configuration.GetSection(nameof(SecretSigningKey)).Get<SecretSigningKey>() ?? null;


	public SettingsProviderService(IConfiguration configuration, EnvironmentProvider environmentProvider)
	{
		_configuration = configuration;
		_environment = environmentProvider;
	}
}

