using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.ComponentModel.DataAnnotations;
using vdb_node_api.Models.Api;
using vdb_node_api.Services;

namespace vdb_node_api.Controllers;

[ApiController]
[AllowAnonymous]
[Route("api/[controller]")]
[Consumes("application/json")]
[Produces("application/json")]
public sealed class PeersController : ControllerBase
{
	private readonly PeersBackgroundService _peersService;
	private readonly EnvironmentProvider _environment;
	private readonly ILogger<PeersController> _logger;
	public PeersController(
		PeersBackgroundService peersService,
		EnvironmentProvider environmentProvider,
		ILogger<PeersController> logger)
	{
		_peersService = peersService;
		_environment = environmentProvider;
		_logger = logger;
	}

	[HttpGet]
	public async Task<IActionResult> GetPeersList([FromQuery] bool keysOnly = false)
	{
		if(_environment.DISABLE_GET_PEERS ?? false)
			return StatusCode(StatusCodes.Status405MethodNotAllowed);

		if(keysOnly) {
			return StatusCode(StatusCodes.Status200OK, _peersService.GetPublicKeys());
		} else {
			/* This task may take extremely long time, so you should run it async only.
			 * Consider being ready for timeout of any kind (i.e. 504 gateway timeout).
			 */
			return await Task.Run(() => StatusCode(StatusCodes.Status200OK, 
				this.IncapsulateEnumerator(_peersService.GetPeersAndUpdateState())));
		}
	}

	[HttpPut]
	public async Task<IActionResult> AddPeer([Required][FromBody] PeerActionRequest request)
	{
		if(!this.ValidatePubkey(request.PublicKey)) {
			_logger.LogWarning($"Invalid pubkey provided: \'{request.PublicKey}\'.");
			return StatusCode(StatusCodes.Status400BadRequest, ErrorMessages.WireguardPublicKeyFormatInvalid);
		}

		try {
			string ip = await _peersService.EnsurePeerAdded(request.PublicKey);
			_logger.LogInformation($"Successfully added new peer on \'{ip}\': \'{request.PublicKey}\'.");
			return StatusCode(StatusCodes.Status200OK, request.CreateResponse(ip, _peersService.InterfacePubkey));
		} catch(Exception ex) {
			_logger.LogWarning($"Could not add new peer: \'{ex.Message}\'. Pubkey was: \'{request.PublicKey}\'.");
			try {
				// no need to catch
				await _peersService.EnsurePeerRemoved(request.PublicKey);
			} catch { }
			return StatusCode(StatusCodes.Status500InternalServerError);
		}
	}

	[HttpPatch]
	public async Task<IActionResult> DeletePeer([Required][FromBody] PeerActionRequest request)
	{
		if(_environment.DISABLE_DELETE_PEERS ?? false)
			return StatusCode(StatusCodes.Status405MethodNotAllowed);

		if(!this.ValidatePubkey(request.PublicKey)) {
			_logger.LogWarning($"Invalid pubkey provided: \'{request.PublicKey}\'.");
			return StatusCode(StatusCodes.Status400BadRequest, ErrorMessages.WireguardPublicKeyFormatInvalid);
		}

		try {
			bool removed = await _peersService.EnsurePeerRemoved(request.PublicKey);

			if(removed) {
				_logger.LogInformation($"Successfully removed peer: \'{request.PublicKey}\'.");
				return StatusCode(StatusCodes.Status200OK);
			} else {
				_logger.LogWarning($"Peer requested for deletion was not found: \'{request.PublicKey}\'.");
				return StatusCode(StatusCodes.Status404NotFound);
			}
		} catch(Exception ex) {
			_logger.LogWarning($"Could not delete peer: \'{ex.Message}\'. Pubkey was: \'{request.PublicKey}\'.");
			return StatusCode(StatusCodes.Status500InternalServerError);
		}
	}
}

