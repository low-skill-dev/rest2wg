using Microsoft.AspNetCore.Mvc;
using vdb_node_api.Services;
using System.Security.Cryptography;
using vdb_node_api.Models;

namespace vdb_node_api.Infrastructure
{

	public sealed class AuthorizationMiddleware : IMiddleware
	{
		private readonly MasterAccountsService _accountsService;
		private readonly ILogger _logger;
		public AuthorizationMiddleware(MasterAccountsService accountsService, ILogger<AuthorizationMiddleware> logger)
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
					GetRejectionMessage(context, "Authorization header appeared more than once."));

				return Task.FromResult(new StatusCodeResult(StatusCodes.Status400BadRequest));
			}

			if (string.IsNullOrEmpty(key)) // key is not present
			{
				_logger.LogWarning(
					GetRejectionMessage(context, "Authorization key was null or empty."));

				return Task.FromResult(new StatusCodeResult(StatusCodes.Status400BadRequest));
			}

			try
			{
				if (!_accountsService.IsValid(key)) // key format is valid, but not found
				{
					_logger.LogWarning(
						GetRejectionMessage(context, "Authorization key was not found on the server."));

					return Task.FromResult(new StatusCodeResult(StatusCodes.Status401Unauthorized));
				}
			}
			catch // key format is invalid
			{
				_logger.LogWarning(
						GetRejectionMessage(context, "Authorization key format was not valid."));

				return Task.FromResult(new StatusCodeResult(StatusCodes.Status400BadRequest));
			}

			// key was successfully validated
			return next(context);
		}
	}
}
