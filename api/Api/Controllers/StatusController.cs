using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;

namespace Api.Controllers;

[ApiController]
[AllowAnonymous]
[Route("api/[controller]")]
[Consumes("application/json")]
[Produces("application/json")]
public sealed class StatusController : ControllerBase
{
	[HttpGet]
	public async Task<IActionResult> GetStatus()
	{
		return await Task.FromResult(StatusCode(StatusCodes.Status200OK));
	}
}