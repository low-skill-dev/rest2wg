using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Cryptography;
using vdb_node_api.Models.Api;
using vdb_node_api.Models.Runtime;
using vdb_node_api.Services;
using vdb_node_wireguard_manipulator;

namespace vdb_node_api.Controllers;

[ApiController]
[AllowAnonymous]
[Route("api/[controller]")]
[Consumes("application/json")]
[Produces("application/json")]
public sealed class StatusController : ControllerBase
{
	private readonly bool _allowNoAuth;
	private readonly bool _disableHmac;
	private string? _keyForHmac512;
	private readonly ILogger<StatusController> _logger;
	public StatusController(
		SettingsProviderService settingsProviderService, 
		EnvironmentProvider environmentProvider, 
		ILogger<StatusController> logger)
	{
		_allowNoAuth = environmentProvider.ALLOW_NOAUTH ?? false;
		_disableHmac = environmentProvider.DISABLE_STATUS_HMAC ?? false;
		_keyForHmac512 = settingsProviderService.SecretSigningKey?.KeyBase64;

		_logger = logger;
	}

	[HttpGet]
	public IActionResult GetStatus()
	{
		if(!_disableHmac && !_allowNoAuth && _keyForHmac512 is not null) {
			byte[] hmacKey;
			try {
				hmacKey = Convert.FromBase64String(_keyForHmac512);
			} catch {
				// this will disable HMAC'ing if there is no way to perform it
				_keyForHmac512 = null;
				_logger.LogError("Invalid base64-encoded key provided for GET:/api/status " +
					"endpoint HMAC'ing is ignored now on.");
				return GetStatus();
			}

			// Authorization middleware (enabled by !_allowNoAuth) already validated that there is a single key
			var authKey = Convert.FromBase64String(Request.Headers.Authorization.Single()!.Split(' ').Last());
			return StatusCode(StatusCodes.Status200OK,
				new SecuredStatusResponse(Convert.ToBase64String(HMACSHA512.HashData(hmacKey, authKey))));
		} else {
			return StatusCode(StatusCodes.Status200OK);
		}
	}
}