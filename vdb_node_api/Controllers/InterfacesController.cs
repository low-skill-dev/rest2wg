using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.ComponentModel.DataAnnotations;
using vdb_node_api.Models;
using vdb_node_api.Services;

namespace vdb_node_api.Controllers;

[AllowAnonymous]
[ApiController]
[Route("api/[controller]")]
[Consumes("application/json")]
[Produces("application/json")]
public class InterfacesController : ControllerBase
{
	protected readonly ILogger<PeersController> _logger;
	protected readonly PeersBackgroundService _peersService;
	public InterfacesController(PeersBackgroundService peersService, ILogger<PeersController> logger)
	{
		_logger = logger;
		_peersService = peersService;
	}

	[HttpGet]
	public async Task<IActionResult> GetInterfacesList()
	{
		return Ok(await _peersService.GetInterfaces());
	}

	[HttpPost]
	public async Task<IActionResult> GetInterfaceInfo([Required][FromBody] InterfaceActionRequest request)
	{
		if (!this.ValidatePubkey(request.PublicKey))
		{
			_logger.LogWarning($"Invalid pubkey provided. Pubkey was: {request.PublicKey}.");
			return BadRequest("Pubkey format is invalid");
		}

		var result = await _peersService.GetInterfaceInfo(request.PublicKey);
		return result is null ? NotFound() : Ok(result);
	}
}

