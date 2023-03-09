using Microsoft.Extensions.Logging;
using System.Reflection.Metadata;

namespace vdb_node_api.Services;

public sealed class EnvironmentProvider
{
	const string ENV_ALLOW_NOAUTH = "REST2WG_ALLOW_NOAUTH";
	const string ENV_AUTH_KEYHASH = "REST2WG_AUTH_KEYHASH_BASE64";
	const string ENV_HANDSHAKE_AGO_LIMIT = "REST2WG_HANDSHAKE_AGO_LIMIT";
	const string ENV_REVIEW_INTERVAL = "REST2WG_REVIEW_INTERVAL";

	public bool? ALLOW_NOAUTH { get; init; } = null;
	public string? AUTH_KEYHASH { get; init; } = null;
	public int? HANDSHAKE_AGO_LIMIT { get; init; } = null;
	public int? REVIEW_INTERVAL { get; init; } = null;

	private ILogger<EnvironmentProvider> _logger;

	public EnvironmentProvider(ILogger<EnvironmentProvider> logger)
	{
		_logger = logger;

		ALLOW_NOAUTH = ParseBoolValue(ENV_ALLOW_NOAUTH);
		AUTH_KEYHASH = ParseStringValue(ENV_AUTH_KEYHASH, 
			s=> s.Length <1024 && Convert.TryFromBase64String(s,new byte[1024], out _))!;
		HANDSHAKE_AGO_LIMIT = ParseIntValue(ENV_HANDSHAKE_AGO_LIMIT);
		REVIEW_INTERVAL = ParseIntValue(ENV_REVIEW_INTERVAL);
	}

	private string GetIncorrectIgnoredMessage(string EnvName)
	{
		return $"Incorrect valued of {ENV_REVIEW_INTERVAL} environment variable value was ignored.";
	}

	private bool? ParseBoolValue(string EnvName)
	{
		var str = Environment.GetEnvironmentVariable(EnvName);
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
		return null;
	}
	private int? ParseIntValue(string EnvName, int minValue=int.MinValue)
	{
		var str = Environment.GetEnvironmentVariable(EnvName);
		if (str is not null)
		{
			if (int.TryParse(str, out var val) && val >= minValue)
			{
				_logger.LogInformation($"{EnvName}={val}.");
			}
			_logger.LogWarning(GetIncorrectIgnoredMessage(EnvName));
		}
		return null;
	}

	private string? ParseStringValue(string EnvName, Func<string,bool> valueValidator)
	{
		var str = Environment.GetEnvironmentVariable(EnvName);
		if (str is not null)
		{
			if (valueValidator(str))
			{
				_logger.LogInformation($"{EnvName}={str}.");
				return str;
			}
			_logger.LogWarning(GetIncorrectIgnoredMessage(EnvName));
		}
		return null;
	}
}


