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
public sealed class StatusController : ControllerBase
{
	[HttpGet] public IActionResult GetStatus() => Ok();
}