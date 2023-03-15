using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Cryptography;
using vdb_node_api.Models.Api;
using vdb_node_api.Models.Runtime;
using vdb_node_api.Services;
using vdb_node_wireguard_manipulator;

namespace vdb_node_api.Controllers;

[AllowAnonymous]
[ApiController]
[Route("api/[controller]")]
[Consumes("application/json")]
[Produces("application/json")]
public sealed class StatusController : ControllerBase
{
	private bool _disableHmac;
	private bool _allowNoAuth;
	private SecretSigningKey? _keyForHmac512;
	public StatusController(SettingsProviderService settingsProviderService, EnvironmentProvider environmentProvider)
	{
		_disableHmac = environmentProvider.DISABLE_STATUS_HMAC ?? false;
		_keyForHmac512 = settingsProviderService.SecretSigningKey;
		_allowNoAuth = environmentProvider.ALLOW_NOAUTH ?? false;
	}

	[HttpGet]
	public IActionResult GetStatus()
	{
		if (!_disableHmac && !_allowNoAuth && _keyForHmac512 is not null)
		{
			byte[] hmacKey;
			try
			{
				hmacKey = Convert.FromBase64String(_keyForHmac512.KeyBase64);
			}
			catch
			{
				return Problem();
			}

			// Authorization middleware (enabled by !_allowNoAuth) already validated that there is a single key
			var authKey = Convert.FromBase64String(Request.Headers.Authorization.Single()!.Split(' ').Last());

			var hmac = HMACSHA512.HashData(hmacKey, authKey);

			return Ok(new SecuredStatusResponse(Convert.ToBase64String(hmac)));
		}
		else
		{
			return Ok();
		}
	}
}