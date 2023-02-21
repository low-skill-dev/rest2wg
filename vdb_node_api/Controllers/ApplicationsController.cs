using IdentityModel;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.ComponentModel.DataAnnotations;
using System.Security.Claims;
using System.Security.Cryptography;
using vdb_node_api.Infrastructure.Database;
using vdb_node_api.Models.Api.Application;
using vdb_node_api.Models.Api.Master;
using vdb_node_api.Models.Database;
using vdb_node_api.Services;

namespace vdb_node_api.Controllers
{
	/* Итак, после проведения минимального прототипирования и наброски кода, 
	 * были приняты некоторые архитектурные решения. Для приложения определяются 
	 * некоторые мастер-аккаунты, которые записываются в файл секретов и передаются 
	 * приложению в момент развертывания. Данные аккаунты имеют доступ к созданию 
	 * обычных аккаунтов – которые непосредственно манипулируют Wireguard-сервисом. 
	 * Во время исполнения невозможно добавление мастер-аккаунтов, но возможна их 
	 * блокировка до перезапуска приложения. Каждый мастер-аккаунт от своего имени 
	 * может заблокировать либо только себя, либо все мастер-аккаунты. Обычные аккаунты 
	 * хранятся в качестве записей в базе данных.
	 */

	[AllowAnonymous]
	[ApiController]
	[Route("{controller}")]
	public sealed class ApplicationsController : ControllerBase
	{
		private readonly ILogger<ApplicationsController> _logger;
		private readonly VdbNodeContext _context;
		private readonly JwtService _jwtService;
		private readonly ApplicationAccountService _accountService;
		private readonly MasterAccountService _mastersService;

		public ApplicationsController(
			JwtService jwtService, ApplicationAccountService accountService,
			MasterAccountService mastersService, VdbNodeContext vdbNodeContext,
			ILogger<ApplicationsController> logger)
		{
			_jwtService = jwtService;
			_accountService = accountService;
			_mastersService = mastersService;
			_context = vdbNodeContext;
			_logger = logger;
		}
		/// <returns>
		/// ApplicationAccount if apiKey was found in database,<br/>
		/// null otherwise.
		/// </returns>
		[NonAction]
		private async Task<ApplicationAccount?> findAppByKey(string? apiKey)
		{
			if (string.IsNullOrEmpty(apiKey)) return null;

			var keyHash = Base64Url.Encode(SHA512.HashData(Base64Url.Decode(apiKey)));
			return await _context.ApplicationAccounts.FirstOrDefaultAsync(x => x.ApiKeyHash.Equals(keyHash));
		}

		#region ValidateApplicationJwt 
		/* Метод служит для валидации JWT токена пользователя API.
		 * Данный метод не выполняет реальной работы, а лишь подтверждает, 
		 * что мидлваре, отвечающее за авторизацию, пропустило запрос.
		 * При том метод не осуществляет фактическую проверку действительности
		 * ключа, это должно произойти при смерти краткосрочного JWT.
		 */
		[HttpGet, Authorize]
		[Route("validate")]
		public async Task<IActionResult> ValidateApplicationJwt()
		{
			// I hate warnings, I hate warnings, I hate warnings... I HATE THIS GREEN LINES I HATE WARNINGS !!!!!!!
			return await Task.FromResult(Ok());
		}
		#endregion

		/* Метод служит для получения JWT токена пользователя API.
		 */
		[HttpPost, AllowAnonymous]
		[Route("login")]
		public async Task<IActionResult> LoginApplication([Required][FromBody] LoginRequest request)
		{
			var found = await findAppByKey(request.ApiKey);
			if (found is null)
			{
				return Unauthorized();
			}

			var generatedJwt = _jwtService.GenerateAccessToken(found);
			return Ok(new LoginResponse(generatedJwt));
		}

		/* Метод предназначен для http-only безопасного обновления.
		 */
		[HttpPost, AllowAnonymous]
		[Route("refresh")]
		public async Task<IActionResult> RefreshJwtToken()
		{
			if (!Request.Cookies.ContainsKey(JwtService.RefreshTokenCookieName))
			{
				return BadRequest("Refresh token cookie was not present.");
			}

			return await RefreshJwtToken(new(Request.Cookies[JwtService.RefreshTokenCookieName] ?? string.Empty));
		}
		
		[HttpPost, AllowAnonymous]
		[Route("refresh-frombody")]
		public async Task<IActionResult> RefreshJwtToken([FromBody][Required] RefreshJwtFrombodyRequest request)
		{
			var refreshJwt = request.RefreshJwt;
			if (string.IsNullOrEmpty(refreshJwt))
			{
				return BadRequest("Refresh token was not present.");
			}

			ClaimsPrincipal? parsedJwt;
			try
			{
				parsedJwt = _jwtService.ValidateJwtToken(refreshJwt);
			}
			catch
			{
				return BadRequest("Refresh token was corrupted.");
			}

			var idClaim = parsedJwt.FindFirstValue(nameof(ApplicationAccount.Id));
			var keyClaim = parsedJwt.FindFirstValue(JwtService.RefreshKeyFieldName);
			if (idClaim is null || keyClaim is null)
			{
				return BadRequest("Refresh token does not contain all of required fields.");
			}

			if (!long.TryParse(idClaim, out var accoutId))
			{
				return BadRequest("Refresh token fields was corrupted.");
			}

			var account = await _context.ApplicationAccounts.FirstOrDefaultAsync(x => x.Id == accoutId);
			if (account is null)
			{
				return BadRequest("Refreshed account was not found on the server.");
			}

			string keyHash;
			try
			{
				keyHash = Base64Url.Encode(SHA512.HashData(Base64Url.Decode(keyClaim)));
			}
			catch
			{
				return BadRequest("Refresh key was in a wrong format");
			}

			var keyId = account.RefreshJwtKeysHashes.ToList().FindIndex(k => k.Equals(keyHash));
			if (keyId < 0)
			{
				return BadRequest("Refresh key was not valid for this account.");
			}

			// all checks completed here

			var newAccess = _jwtService.GenerateAccessToken(account);
			var newRefresh = _jwtService.GenerateRefreshToken(account, out var newKey, out var newHash);

			account.RefreshJwtKeysHashes[keyId] = newHash;
			try
			{
				await _context.SaveChangesAsync();
			}
			catch
			{
				return StatusCode(StatusCodes.Status500InternalServerError);
			}


			return Ok(new LoginResponse(newAccess, newRefresh));
		}

	}
}
