using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.ComponentModel.DataAnnotations;
using System.Security.Claims;
using System.Security.Cryptography;
using vdb_node_api.Models;
using vdb_node_api.Services;
using vdb_node_wireguard_manipulator;

namespace vdb_node_api.Controllers;




[AllowAnonymous]
[ApiController]
[Route("api/[controller]")]
[Consumes("application/json")]
[Produces("application/json")]
public sealed class PeersController : ControllerBase
{
	private readonly ILogger<PeersController> _logger;
	private readonly PeersBackgroundService _peersService;
	public PeersController(PeersBackgroundService peersService, ILogger<PeersController> logger)
	{
		_logger = logger;
		_peersService = peersService;
	}

	/* DONE: Create injection protection.
	 * Add validating that the passed peers public key is 
	 * actually a base64-encoded string, not somethig 
	 * like ...; wg-quick down wg0;...
	 */


	[HttpGet]
	public async Task<IActionResult> GetPeersList()
	{
		return Ok(await Task.Run(()=>_peersService.GetPeers()));
	}

	[HttpPost]
	public async Task<IActionResult> GetPeerInfo([Required][FromBody] PeerActionRequest request)
	{
		if (!this.ValidatePubkey(request.PublicKey))
		{
			_logger.LogWarning($"Invalid pubkey provided. Pubkey was: {request.PublicKey}.");
			return BadRequest("Pubkey format is invalid");
		}

		var result = await _peersService.GetPeerInfo(request.PublicKey);
		return result is null ? NotFound() : Ok(result);
	}

	[HttpPut]
	public async Task<IActionResult> AddPeer([Required][FromBody] PeerActionRequest request)
	{
		if (!this.ValidatePubkey(request.PublicKey))
		{
			_logger.LogWarning($"Invalid pubkey provided. Pubkey was: {request.PublicKey}.");
			return BadRequest("Pubkey format is invalid");
		}

		try
		{
			var ip = await _peersService.AddPeer(request.PublicKey);
			_logger.LogInformation($"Successfully added new peer {request.PublicKey} on {ip}.");
			return Ok(
				request.CreateAddResponse(ip, 
				(await _peersService.GetInterfaces()).SingleOrDefault()?.PublicKey));
		}
		catch (Exception ex)
		{
			_logger.LogWarning($"Could not add new peer: {ex.Message}. Pubkey was: {request.PublicKey}.");
			try
			{ // its ok if this throws, no need to catch
				await _peersService.DeletePeer(request.PublicKey);
			}
			catch { }
			return Problem(
#if DEBUG
				ex.Message,
#endif
				statusCode: 500);
		}
	}

	[HttpDelete]
	public async Task<IActionResult> DeletePeer([Required][FromBody] PeerActionRequest request)
	{
		if (!this.ValidatePubkey(request.PublicKey))
		{
			_logger.LogWarning($"Invalid pubkey provided. Pubkey was: {request.PublicKey}.");
			return BadRequest("Pubkey format is invalid");
		}

		try
		{
			await _peersService.DeletePeer(request.PublicKey);
			_logger.LogInformation($"Successfully deleted peer {request.PublicKey}.");
			return Ok();
		}
		catch (Exception ex)
		{
			_logger.LogWarning($"Could not delete peer: {ex.Message}. Pubkey was: {request.PublicKey}.");
			return Problem(
#if DEBUG
				ex.Message,
#endif
				statusCode: 500);
		}
	}
}

