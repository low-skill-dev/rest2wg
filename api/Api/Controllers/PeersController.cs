using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.ComponentModel.DataAnnotations;
using Api.Services;
using WgManipulator;
using Models;

namespace Api.Controllers;

[ApiController]
[AllowAnonymous]
[Route("api/[controller]")]
[Consumes("application/json")]
[Produces("application/json")]
public sealed class PeersController : ControllerBase
{
	private readonly IWgManipulator _wg;
	private readonly IpDedicationService _ip;
	private readonly EnvironmentProvider _envProvider;
	private readonly ILogger<PeersController> _logger;

	public PeersController(
		IWgManipulator wg,
		IpDedicationService ip,
		EnvironmentProvider envProvider,
		ILogger<PeersController> logger)
	{
		_wg = wg;
		_ip = ip;
		_envProvider = envProvider; _envProvider = envProvider;
		_logger = logger;
	}

	[NonAction]
	public bool ValidateWgPubkey(string pk, int strictBytesCount = 256 / 8, bool withLog = true)
	{
		var ok = !string.IsNullOrWhiteSpace(pk)
			&& pk.Length <= (strictBytesCount * 4 / 3 + 3)
			&& Convert.TryFromBase64String(pk, new byte[strictBytesCount], out var bytesCount)
			&& (strictBytesCount < 0 || bytesCount == strictBytesCount);

		if(!ok && withLog)
			_logger.LogWarning($"Invalid pubkey provided in the request: \'{pk}\'.");

		return ok;
	}

	[HttpGet]
	public async Task<IActionResult> GetPeers([FromQuery] bool withCleanup = false)
	{
		return Ok(await _wg.GetPeers());
	}

	[HttpPut]
	public async Task<IActionResult> AddPeer([Required][FromBody] WgAddPeerRequest request)
	{
		if(!this.ValidateWgPubkey(request.Pubkey))
			return StatusCode(StatusCodes.Status400BadRequest, ErrorMessages.WireguardPublicKeyFormatInvalid);

		try
		{
			var ip = _ip.EnsureDedicatedAddressForPeer(request.Pubkey);
			var cnt = await _wg.AddPeers([new WgAddPeerRequest { Pubkey = request.Pubkey, AllowedIp = ip }]);
			if(cnt != 1) throw new AggregateException("Wireguard internal error.");

			_logger.LogInformation($"Successfully added new peer on \'{ip}\': \'{request.Pubkey}\'.");
			return StatusCode(StatusCodes.Status200OK, new WgAddPeerResponse(ip, (await _wg.GetInterfaceInfo()).Pubkey));
		}
		catch(Exception ex)
		{
			_logger.LogError($"Error adding new peer: \'{ex.Message}\'. Pubkey: \'{request.Pubkey}\'.");
			try
			{
				// no need to catch
				_ip.DeletePeer(request.Pubkey);
				await _wg.DeletePeers([request.Pubkey]);
			}
			catch { }
			return StatusCode(StatusCodes.Status500InternalServerError, ErrorMessages.InternalErrorAddingNewPeer);
		}
	}

	[HttpDelete]
	public async Task<IActionResult> DeletePeer([Required][FromBody] WgAddPeerRequest request)
	{
		if(!this.ValidateWgPubkey(request.Pubkey))
			return StatusCode(StatusCodes.Status400BadRequest, ErrorMessages.WireguardPublicKeyFormatInvalid);

		try
		{
			var removed = await _wg.DeletePeers([request.Pubkey]);
			_ip.DeletePeer(request.Pubkey);

			if(removed.Count > 0)
			{
				_logger.LogInformation($"Successfully deleted peer: \'{request.Pubkey}\'.");
				return StatusCode(StatusCodes.Status200OK, removed[0]);
			}
			else
			{
				_logger.LogWarning($"Peer requested for deletion was not found: \'{request.Pubkey}\'.");
				return StatusCode(StatusCodes.Status404NotFound,ErrorMessages.PeerPubkeyNotFound);
			}
		}
		catch(Exception ex)
		{
			_logger.LogError($"Could not delete peer: \'{ex.Message}\'. Pubkey was: \'{request.Pubkey}\'.");
			return StatusCode(StatusCodes.Status500InternalServerError, ErrorMessages.InternalErrorDeletingPeer);
		}
	}

	[HttpDelete]
	[Route("{lastHandhsakeAgoSecondsLimit:long}")]
	public async Task<IActionResult> DeletePeersByHandshakeTime(long lastHandhsakeAgoSecondsLimit)
	{
		var peers = await _wg.GetPeers();

		var now = DateTime.UtcNow;
		var toBeRemoved = peers.Where(x => (now - x.LastHandshake).TotalSeconds > lastHandhsakeAgoSecondsLimit);

		var removed = await _wg.DeletePeers(toBeRemoved.Select(x => x.Pubkey).ToList());

		_logger.LogInformation($"Deleted {removed.Count} peers by hanshake time query: " +
			$"[{string.Join(',', removed.Select(x => $"'{x.Pubkey}'"))}].");

		 return Ok(removed);
	}
}

