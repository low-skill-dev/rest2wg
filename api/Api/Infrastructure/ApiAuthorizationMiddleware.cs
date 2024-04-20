using Api.Services;
using System.Security.Cryptography;

namespace Api.Infrastructure;

public sealed class ApiAuthorizationMiddleware : IMiddleware
{
	private readonly ILogger _logger;
	private readonly byte[]? _accessKeyHash;
	private readonly bool _ignoreUnauthorized;
	public ApiAuthorizationMiddleware(EnvironmentProvider ep, SettingsProviderService sp, ILogger<ApiAuthorizationMiddleware> logger)
	{
		_logger = logger;
		_accessKeyHash = sp.AccessKeyHash;
		_ignoreUnauthorized = ep.IGNORE_UNAUTHORIZED ?? false;
	}

	private string GetRejectionMessage(HttpContext context, string reason)
	{
		return $"Request {context.Connection.Id} from " +
			$"{context.Connection.RemoteIpAddress} rejected: {reason}";
	}

	public Task InvokeAsync(HttpContext context, RequestDelegate next)
	{
		if(_accessKeyHash is null)
		{
			_logger.LogWarning(GetRejectionMessage(context,
				$"Auth key hash was not found in config. " +
				$"It is required if authorization is enabled."));

			context.Response.StatusCode = _ignoreUnauthorized
				? StatusCodes.Status403Forbidden
				: StatusCodes.Status500InternalServerError;
			return Task.CompletedTask;
		}

		var header = context.Request.Headers.Authorization;
		string? key;
		try
		{
			/* according to RFC, the header may have single value only
			 * Authorization  = "Authorization" ":" credentials
			 * https://www.w3.org/Protocols/rfc2616/rfc2616-sec14.html
			 */
			key = header.Single();
		}
		catch(InvalidOperationException) // key is not single
		{
			_logger.LogWarning(GetRejectionMessage(context,
				$"Authorization header appeared {header.Count} times, expected 1."));

			context.Response.StatusCode = _ignoreUnauthorized
				? StatusCodes.Status403Forbidden
				: StatusCodes.Status400BadRequest;
			return Task.CompletedTask;
		}

		if(string.IsNullOrEmpty(key)) // key is not present
		{
			_logger.LogWarning(GetRejectionMessage(context,
				"Authorization key was empty."));

			context.Response.StatusCode = _ignoreUnauthorized
				? StatusCodes.Status403Forbidden
				: StatusCodes.Status400BadRequest;
			return Task.CompletedTask;
		}

		try
		{
			key = key.Split(' ').Last();

			byte[] keyHash = SHA512.HashData(Convert.FromBase64String(key));
			_accessKeyHash.SequenceEqual(keyHash);

			if(_accessKeyHash.SequenceEqual(keyHash)) // wrong key
			{
				context.Response.StatusCode = _ignoreUnauthorized
					? StatusCodes.Status403Forbidden
					: StatusCodes.Status401Unauthorized;
				return Task.CompletedTask;
			}
		}
		catch // invalid key format
		{
			_logger.LogWarning(GetRejectionMessage(context, 
				"Authorization key format was not valid."));

			context.Response.StatusCode = _ignoreUnauthorized
				? StatusCodes.Status403Forbidden
				: StatusCodes.Status400BadRequest;
			return Task.CompletedTask;
		}

		// key was successfully validated
		return next(context);
	}
}
