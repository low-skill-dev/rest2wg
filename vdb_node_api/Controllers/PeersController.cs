using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.ComponentModel.DataAnnotations;
using System.Security.Claims;
using System.Security.Cryptography;
using vdb_node_api.Models;
using vdb_node_api.Services;
using vdb_node_wireguard_manipulator;

namespace vdb_node_api.Controllers
{
	/* TODO: Create injection protection by validateing than the
	 * passed public key as actuaaly a base64-encoded string,
	 * not somethig like ...; wg-quick down wg0;...
	 */

	[AllowAnonymous]
	[ApiController]
	[Route("api/[controller]")]
	[Consumes("application/json")]
	[Produces("application/json")]
	public sealed class  PeersController : ControllerBase
	{
		private readonly IpDedicationService _ipService;
		private readonly ILogger<PeersController> _logger;
		public PeersController(IpDedicationService ipService, ILogger<PeersController> logger)
		{
			_ipService = ipService;
			_logger = logger;
		}

		[HttpGet]
		[Route("{pubKey}")]
		public async Task<IActionResult> GetPeerInfo([Required][FromRoute] string pubKey)
		{
			// manipulate wireguard here
			throw new NotImplementedException();
		}

		[HttpPut]
		public async Task<IActionResult> AddPeer([Required][FromBody] PeerActionRequest request)
		{
			var ip = _ipService.EnsureDedicatedAddressForPeer(request.PublicKey);
			var executor = new CommandsExecutor();

			if (string.IsNullOrEmpty(request.PublicKey))
			{
				return BadRequest();
			}

			var result = await executor.AddPeer(request.PublicKey, ip);

			if (string.IsNullOrWhiteSpace(result)) // wg set wg0 peer commonly has no output
			{
				return Ok();
			}
			else
			{
				return Problem(result, statusCode: 500);
			}
		}

		[HttpDelete]
		public async Task<IActionResult> DeletePeer([Required][FromBody] PeerActionRequest request)
		{
			// manipulate wireguard here
			throw new NotImplementedException();
		}
	}
}
