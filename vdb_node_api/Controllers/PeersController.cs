using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.ComponentModel.DataAnnotations;
using vdb_node_api.Models.Api;
using vdb_node_api.Services;

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
	private readonly EnvironmentProvider _environment;
	public PeersController(
		PeersBackgroundService peersService, 
		EnvironmentProvider environmentProvider, 
		ILogger<PeersController> logger)
	{
		_logger = logger;
		_peersService = peersService;
		_environment = environmentProvider;
	}

	[HttpGet]
	public async Task<IActionResult> GetPeersList()
	{
		if (_environment.DISABLE_GET_PEERS ?? false) 
			return StatusCode(StatusCodes.Status405MethodNotAllowed);

		return await Task.Run(()=>Ok(this.IncapsulateEnumerator(_peersService.GetPeersAndUpdateState())));
	}

	[HttpPut]
	public async Task<IActionResult> AddPeer([Required][FromBody] PeerActionRequest request)
	{
		if (!this.ValidatePubkey(request.PublicKey))
		{
			_logger.LogWarning($"Invalid pubkey provided. Pubkey was: {request.PublicKey}.");
			return BadRequest("Pubkey format is invalid.");
		}

		try
		{
			string ip = await _peersService.EnsurePeerAdded(request.PublicKey);
			_logger.LogInformation($"Successfully added new peer {request.PublicKey} on {ip}.");
			return Ok(request.CreateResponse(ip, _peersService.InterfacePubkey));
		}
		catch (Exception ex)
		{
			_logger.LogWarning($"Could not add new peer: {ex.Message}. Pubkey was: {request.PublicKey}.");
			try
			{ // its ok if this throws, no need to catch
				await _peersService.EnsurePeerRemoved(request.PublicKey);
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
		if (_environment.DISABLE_DELETE_PEERS ?? false)
			return StatusCode(StatusCodes.Status405MethodNotAllowed);

		if (!this.ValidatePubkey(request.PublicKey))
		{
			_logger.LogWarning($"Invalid pubkey provided. Pubkey was: {request.PublicKey}.");
			return BadRequest("Pubkey format is invalid.");
		}

		try
		{
			bool removed = await _peersService.EnsurePeerRemoved(request.PublicKey);

			_logger.LogInformation(removed ? $"Successfully removed peer {request.PublicKey}."
				: $"Peer requested for deletion is not found. Pubkey was: {request.PublicKey}.");
			return removed ? Ok() : NotFound();
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

