namespace Api.Services;

public sealed class EnvironmentProvider
{
	private const string ENV_ALLOW_NOAUTH = "ALLOW_NOAUTH";
	private const string ENV_AUTH_KEYHASH = "REST2WG_AUTH_KEYHASH_BASE64";
	private const string ENV_HANDSHAKE_AGO_LIMIT = "REST2WG_HANDSHAKE_AGO_LIMIT";
	private const string ENV_IGNORE_UNATHORIZED = "REST2WG_IGNORE_UNAUTHORIZED";

	public bool? ALLOW_NOAUTH { get; init; } = null;
	public string? AUTH_KEYHASH { get; init; } = null;
	public int? HANDSHAKE_AGO_LIMIT { get; init; } = null;
	public bool? IGNORE_UNAUTHORIZED { get; init; } = null;

	private readonly ILogger<EnvironmentProvider> _logger;

	public EnvironmentProvider(ILogger<EnvironmentProvider> logger)
	{
		_logger = logger;

		ALLOW_NOAUTH = ParseBoolValue(ENV_ALLOW_NOAUTH);
		AUTH_KEYHASH = ParseStringValue(ENV_AUTH_KEYHASH,
			s => s.Length < 6000 && Convert.TryFromBase64String(s, new byte[s.Length * 4 / 3 + 3], out _));
		HANDSHAKE_AGO_LIMIT = ParseIntValue(ENV_HANDSHAKE_AGO_LIMIT, x => x >= 0);
		IGNORE_UNAUTHORIZED = ParseBoolValue(ENV_IGNORE_UNATHORIZED);
	}

	private string GetIncorrectIgnoredMessage(string EnvName)
	{
		return $"Incorrect value of {EnvName} environment variable was ignored.";
	}

	private bool? ParseBoolValue(string envName)
	{
		string? str = Environment.GetEnvironmentVariable(envName);
		if(str is not null)
		{
			if(str.Equals("true", StringComparison.InvariantCultureIgnoreCase))
			{
				_logger.LogInformation($"{envName}={true}.");
				return true;
			}
			if(str.Equals("false", StringComparison.InvariantCultureIgnoreCase))
			{
				_logger.LogInformation($"{envName}={false}.");
				return false;
			}
			_logger.LogError(GetIncorrectIgnoredMessage(envName));
			return null;
		}
		_logger.LogInformation($"{envName} was not present.");
		return null;
	}
	private int? ParseIntValue(string envName, Func<int, bool>? valueValidator = null)
	{
		string? str = Environment.GetEnvironmentVariable(envName);
		if(str is not null)
		{
			if(int.TryParse(str, out int val) && (valueValidator?.Invoke(val) ?? true))
			{
				_logger.LogInformation($"{envName}={val}.");
				return val;
			}
			_logger.LogError(GetIncorrectIgnoredMessage(envName));
			return null;
		}
		_logger.LogInformation($"{envName} was not present.");
		return null;
	}
	private string? ParseStringValue(string envName, Func<string, bool>? valueValidator = null)
	{
		string? str = Environment.GetEnvironmentVariable(envName);
		if(str is not null)
		{
			if(valueValidator?.Invoke(str) ?? true)
			{
				_logger.LogInformation($"{envName}={str}.");
				return str;
			}
			_logger.LogError(GetIncorrectIgnoredMessage(envName));
			return null;
		}
		_logger.LogInformation($"{envName} was not present.");
		return null;
	}
}


