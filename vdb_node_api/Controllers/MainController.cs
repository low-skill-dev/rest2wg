using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.ComponentModel.DataAnnotations;
using System.Security.Claims;
using System.Security.Cryptography;
using vdb_node_api.Services;

namespace vdb_node_api.Controllers
{
	/* 
	 */

	[AllowAnonymous]
	[ApiController]
	[Route("api")]
	public sealed class  MainController : ControllerBase
	{
		private readonly ILogger<MainController> _logger;
		public MainController(
			ILogger<MainController> logger)
		{
			_logger = logger;
		}

		public async Task<IActionResult> AddPeer()
		{
			throw new NotImplementedException();
		}
	}
}
