using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.ComponentModel.DataAnnotations;
using System.Security.Claims;
using System.Security.Cryptography;
using vdb_node_api.Models;
using vdb_node_api.Services;

namespace vdb_node_api.Controllers
{
	/* 
	 */

	[AllowAnonymous]
	[ApiController]
	[Route("api/{controller}")]
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
		[Route("{pubKey:string}")]
		public async Task<IActionResult> GetPeerInfo([Required][FromRoute] string pubKey)
		{
			// manipulate wireguard here
			throw new NotImplementedException();
		}

		[HttpPut]
		public async Task<IActionResult> AddPeer([Required][FromBody] PeerActionRequest request)
		{
			// manipulate wireguard here
			throw new NotImplementedException();
		}

		[HttpDelete]
		public async Task<IActionResult> DeletePeer([Required][FromBody] PeerActionRequest request)
		{
			// manipulate wireguard here
			throw new NotImplementedException();
		}
	}
}
