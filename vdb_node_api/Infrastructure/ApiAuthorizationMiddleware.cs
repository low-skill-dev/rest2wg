using vdb_node_api.Services;

namespace vdb_node_api.Infrastructure;

public sealed class ApiAuthorizationMiddleware : IMiddleware
{
	private readonly MasterAccountsService _accountsService;
	private readonly ILogger _logger;
	public ApiAuthorizationMiddleware(MasterAccountsService accountsService, ILogger<ApiAuthorizationMiddleware> logger)
	{
		_accountsService = accountsService;
		_logger = logger;
	}

	private string GetRejectionMessage(HttpContext context, string reason)
	{
		return $"Request {context.Connection.Id} from " +
			$"{context.Connection.RemoteIpAddress} rejected: {reason}";
	}

	public Task InvokeAsync(HttpContext context, RequestDelegate next)
	{
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
		catch (InvalidOperationException) // key is not single
		{
			_logger.LogWarning(
				GetRejectionMessage(context, $"Authorization header appeared {header.Count} times, expected 1."));

			context.Response.StatusCode = StatusCodes.Status400BadRequest;
			return Task.CompletedTask;
		}

		if (string.IsNullOrEmpty(key)) // key is not present
		{
			_logger.LogWarning(
				GetRejectionMessage(context, "Authorization key was null or empty."));

			context.Response.StatusCode = StatusCodes.Status400BadRequest;
			return Task.CompletedTask;
		}

		try
		{
			key = key.Split(' ').Last();
			if (!_accountsService.IsValid(key)) // key format is valid, but not found
			{
				_logger.LogWarning(
					GetRejectionMessage(context, "Authorization key was not found on the server."));

				context.Response.StatusCode = StatusCodes.Status401Unauthorized;
				return Task.CompletedTask;
			}
		}
		catch // key format is invalid
		{
			_logger.LogWarning(
					GetRejectionMessage(context, "Authorization key format was not valid."));

			context.Response.StatusCode = StatusCodes.Status400BadRequest;
			return Task.CompletedTask;
		}

		// key was successfully validated
		return next(context);
	}
}
