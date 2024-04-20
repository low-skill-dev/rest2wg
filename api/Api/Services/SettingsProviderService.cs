namespace Api.Services;

public class SettingsProviderService
{
	protected readonly IConfiguration _configuration;
	protected readonly EnvironmentProvider _environment;

	public virtual byte[]? AccessKeyHash
	{
		get
		{
			if(_environment.AUTH_KEYHASH is null) return null;

			var hash = new byte[512];
			Convert.TryFromBase64String(_environment.AUTH_KEYHASH, hash.AsSpan(), out var cnt);

			if(cnt != 512) return null;

			return hash;
		}
	}

	public SettingsProviderService(IConfiguration configuration, EnvironmentProvider environmentProvider)
	{
		_configuration = configuration;
		_environment = environmentProvider;
	}
}

