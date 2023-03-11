namespace vdb_node_api.Services;

public sealed class EnvironmentProvider
{
	private const string ENV_ALLOW_NOAUTH = "REST2WG_ALLOW_NOAUTH";
	private const string ENV_AUTH_KEYHASH = "REST2WG_AUTH_KEYHASH_BASE64";
	private const string ENV_HANDSHAKE_AGO_LIMIT = "REST2WG_HANDSHAKE_AGO_LIMIT";
	private const string ENV_REVIEW_INTERVAL = "REST2WG_REVIEW_INTERVAL";

	public bool? ALLOW_NOAUTH { get; init; } = null;
	public string? AUTH_KEYHASH { get; init; } = null;
	public int? HANDSHAKE_AGO_LIMIT { get; init; } = null;
	public int? REVIEW_INTERVAL { get; init; } = null;

	private readonly ILogger<EnvironmentProvider> _logger;

	public EnvironmentProvider(ILogger<EnvironmentProvider> logger)
	{
		_logger = logger;

		ALLOW_NOAUTH = ParseBoolValue(ENV_ALLOW_NOAUTH);
		AUTH_KEYHASH = ParseStringValue(ENV_AUTH_KEYHASH,
			s => s.Length < 1024 && Convert.TryFromBase64String(s, new byte[1024], out _))!;
		HANDSHAKE_AGO_LIMIT = ParseIntValue(ENV_HANDSHAKE_AGO_LIMIT, 0);
		REVIEW_INTERVAL = ParseIntValue(ENV_REVIEW_INTERVAL, 0);
	}

	private string GetIncorrectIgnoredMessage(string EnvName)
	{
		return $"Incorrect valued of {ENV_REVIEW_INTERVAL} environment variable value was ignored.";
	}

	private bool? ParseBoolValue(string EnvName)
	{
		string? str = Environment.GetEnvironmentVariable(EnvName);
		if (str is not null)
		{
			if (str.Equals("true", StringComparison.InvariantCultureIgnoreCase))
			{
				_logger.LogInformation($"{EnvName}={true}.");
				return true;
			}
			if (str.Equals("false", StringComparison.InvariantCultureIgnoreCase))
			{
				_logger.LogInformation($"{EnvName}={false}.");
				return false;
			}
			_logger.LogWarning(GetIncorrectIgnoredMessage(EnvName));
		}
		_logger.LogInformation($"{EnvName} was not present.");
		return null;
	}
	private int? ParseIntValue(string EnvName, int minValue = int.MinValue)
	{
		string? str = Environment.GetEnvironmentVariable(EnvName);
		if (str is not null)
		{
			if (int.TryParse(str, out int val))
			{
				_logger.LogInformation($"{EnvName}={val}.");
				return val;
			}
			_logger.LogWarning(GetIncorrectIgnoredMessage(EnvName));
		}
		_logger.LogInformation($"{EnvName} was not present.");
		return null;
	}

	private string? ParseStringValue(string EnvName, Func<string, bool> valueValidator)
	{
		string? str = Environment.GetEnvironmentVariable(EnvName);
		if (str is not null)
		{
			if (valueValidator(str))
			{
				_logger.LogInformation($"{EnvName}={str}.");
				return str;
			}
			_logger.LogWarning(GetIncorrectIgnoredMessage(EnvName));
		}
		_logger.LogInformation($"{EnvName} was not present.");
		return null;
	}
}


