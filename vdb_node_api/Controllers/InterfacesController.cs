using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using vdb_node_api.Services;
using vdb_node_wireguard_manipulator;

namespace vdb_node_api.Controllers;

[AllowAnonymous]
[ApiController]
[Route("api/[controller]")]
[Consumes("application/json")]
[Produces("application/json")]
public sealed class InterfacesController : ControllerBase
{
	private readonly ILogger<PeersController> _logger;
	private readonly PeersBackgroundService _peersService;
	public InterfacesController(PeersBackgroundService peersService, ILogger<PeersController> logger)
	{
		_logger = logger;
		_peersService = peersService;
	}

	[HttpGet]
	public IActionResult GetInterfaceInfo()
	{
		return _peersService.InterfacePubkey is null ? NoContent()
			: Ok(new WgInterfaceInfo(_peersService.InterfacePubkey));
	}
}

